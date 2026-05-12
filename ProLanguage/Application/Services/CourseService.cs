using System.Text.Json;
using Application.Constants;
using Application.DTOs.Course;
using Application.Filters;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class CourseService(
    IUnitOfWork unitOfWork,
    ICourseRepository courseRepository,
    IMapper mapper,
    IOptions<CacheSettings> cacheSettings,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    ILogger<CourseService> logger,
    ICourseFilterPipeline filterPipeline,
    ICoursePaginator paginator,
    ICourseImageStorageService courseImageStorageService) : ICourseService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICourseRepository _courseRepository = courseRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<CourseService> _logger = logger;
    private readonly int _cacheExpirationMinutes = cacheSettings.Value.DefaultExpirationMinutes;
    private readonly ICourseFilterPipeline _filterPipeline = filterPipeline;
    private readonly ICoursePaginator _paginator = paginator;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ICourseImageStorageService _courseImageStorageService = courseImageStorageService;

    public async Task<Guid> CreateCourseAsync(CreateCourseDto dto)
    {
        _logger.LogInformation($"Starting course creation for title: {dto.Title}");

        // Check if title already exists
        var titleExists = await _courseRepository.TitleExistsAsync(dto.Title);
        if (titleExists)
        {
            _logger.LogWarning($"Course with title '{dto.Title}' already exists");
            throw new InvalidOperationException($"Course with title '{dto.Title}' already exists.");
        }

        var course = _mapper.Map<Course>(dto);
        if (course is null)
        {
            _logger.LogError($"Course mapping failed - unable to map request to Course entity for title: {dto.Title}");
            throw new InvalidOperationException("Course cannot be null.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Image))
        {
            course.ImageUrl = await _courseImageStorageService.UploadCourseImageAsync(course.Id, dto.Image);
        }

        _logger.LogDebug($"Mapped course DTO to entity for title: {dto.Title}");

        await _courseRepository.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        var courseDto = _mapper.Map<CourseDto>(course);
        _memoryCache.Set(CacheKeys.Courses, courseDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _memoryCache.Remove(CacheKeys.TotalCoursesCount);
        _memoryCache.Remove(CacheKeys.CourseImage(course.Id));
        _logger.LogDebug($"Cleared courses cache after creating course");

        // Bump distributed cache version to invalidate filter caches
        try
        {
            await _distributedCache.SetStringAsync("courses:version", DateTime.UtcNow.Ticks.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to bump courses version in distributed cache");
        }

        _logger.LogInformation($"Successfully created course with ID: {course.Id}, Title: {course.Title}");

        return course.Id;
    }

    public async Task<CourseDto> GetCourseByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving course by ID: {id}");

        if (_memoryCache.TryGetValue(CacheKeys.Courses, out CourseDto? cachedCourse))
        {
            _logger.LogInformation($"Course found in cache for ID: {id}");
            Application.Metrics.BusinessMetrics.CoursesViewedTotal.Inc();
            return cachedCourse;
        }

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {id} not found.");
            throw new KeyNotFoundException($"Course with ID {id} not found.");
        }

        _logger.LogDebug($"Course retrieved from repository for ID: {id}");

        var courseDto = _mapper.Map<CourseDto>(course);
        _memoryCache.Set(CacheKeys.Courses, courseDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Course cached for ID: {id}");

        _logger.LogInformation($"Successfully retrieved course: {id}");
        Application.Metrics.BusinessMetrics.CoursesViewedTotal.Inc();

        return courseDto;
    }

    public async Task<CourseDto?> GetCourseByTitleAsync(string title)
    {
        _logger.LogInformation($"Retrieving course by title: {title}");

        var course = await _courseRepository.GetByTitleAsync(title);
        if (course is null)
        {
            _logger.LogInformation($"Course with title '{title}' not found.");
            return null;
        }

        var courseDto = _mapper.Map<CourseDto>(course);

        _logger.LogInformation($"Successfully retrieved course by title: {title}");

        return courseDto;
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
    {
        _logger.LogInformation("Retrieving all courses");

        var courses = await _courseRepository.GetAllAsync();
        var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);

        _logger.LogInformation($"Successfully retrieved {courseDtos.Count()} courses");

        return courseDtos;
    }

    public async Task<IEnumerable<CourseDto>> GetAvailableCoursesAsync(Guid? excludeLessonId = null)
    {
        _logger.LogInformation($"Retrieving available courses (not at max capacity){(excludeLessonId.HasValue ? $", excluding lesson {excludeLessonId.Value}" : string.Empty)}");

        var courses = await _courseRepository.GetAvailableCoursesAsync(excludeLessonId);
        var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);

        _logger.LogInformation($"Successfully retrieved {courseDtos.Count()} available courses");

        return courseDtos;
    }

    public async Task UpdateCourseAsync(UpdateCourseDto dto)
    {
        _logger.LogInformation($"Starting course update for ID: {dto.Id}");

        var course = await _courseRepository.GetByIdAsync(dto.Id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {dto.Id} not found.");
            throw new KeyNotFoundException($"Course with ID {dto.Id} not found.");
        }

        // Check if title already exists (excluding current course)
        if (!string.IsNullOrEmpty(dto.Title))
        {
            var titleExists = await _courseRepository.TitleExistsAsync(dto.Title, dto.Id);
            if (titleExists)
            {
                _logger.LogWarning($"Course with title '{dto.Title}' already exists");
                throw new InvalidOperationException($"Course with title '{dto.Title}' already exists.");
            }
        }

        _logger.LogDebug($"Found existing course: {course.Id}, Title: {course.Title}");

        _mapper.Map(dto, course);

        if (!string.IsNullOrWhiteSpace(dto.Image))
        {
            var previousImageUrl = course.ImageUrl;
            var newImageUrl = await _courseImageStorageService.UploadCourseImageAsync(course.Id, dto.Image);

            course.ImageUrl = newImageUrl;

            if (!string.IsNullOrWhiteSpace(previousImageUrl))
            {
                await _courseImageStorageService.DeleteImageAsync(previousImageUrl);
            }
        }

        _courseRepository.Update(course);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Courses);
        _memoryCache.Remove(CacheKeys.TotalCoursesCount);
        _memoryCache.Remove(CacheKeys.CourseImage(course.Id));
        _logger.LogDebug($"Cleared courses cache after updating course");

        // Bump distributed cache version to invalidate filter caches
        try
        {
            await _distributedCache.SetStringAsync("courses:version", DateTime.UtcNow.Ticks.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to bump courses version in distributed cache");
        }

        _logger.LogInformation($"Successfully updated course: {course.Id}, Title: {course.Title}");
    }

    public async Task DeleteCourseAsync(Guid id)
    {
        _logger.LogInformation($"Starting course deletion for ID: {id}");

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {id} not found.");
            throw new KeyNotFoundException($"Course with ID {id} not found.");
        }

        if (!string.IsNullOrWhiteSpace(course.ImageUrl))
        {
            await _courseImageStorageService.DeleteImageAsync(course.ImageUrl);
        }

        _courseRepository.Delete(course);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Courses);
        _memoryCache.Remove(CacheKeys.TotalCoursesCount);
        _memoryCache.Remove(CacheKeys.CourseImage(id));
        _logger.LogDebug($"Cleared courses cache after deleting course");

        // Bump distributed cache version to invalidate filter caches
        try
        {
            await _distributedCache.SetStringAsync("courses:version", DateTime.UtcNow.Ticks.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to bump courses version in distributed cache");
        }

        _logger.LogInformation($"Successfully deleted course: {course.Id}");
    }

    public async Task<CourseFilterResultDto> GetFilteredCoursesAsync(CourseFilterDto filter)
    {
        _logger.LogInformation($"Retrieving filtered courses with filter: Page={filter.Page}, PageSize={filter.PageSize}, SortBy={filter.SortBy}");

        // include a distributed 'version' token in cache key so we can invalidate all filter caches
        var version = "0";
        try
        {
            var fetched = await _distributedCache.GetStringAsync("courses:version");
            if (!string.IsNullOrWhiteSpace(fetched))
            {
                version = fetched;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read courses version from distributed cache");
        }

        // Build cache key from filter properties + version
        string cacheKey = $"courses:filter:ver={version}:page={filter.Page}:size={filter.PageSize}:sort={filter.SortBy}:title={filter.Title}:min={filter.MinPrice}:max={filter.MaxPrice}:langs={(filter.Languages is null ? string.Empty : string.Join(',', filter.Languages))}:levels={(filter.Levels is null ? string.Empty : string.Join(',', filter.Levels))}:minr={filter.MinRating}:onSale={filter.OnSale}:mindur={filter.MinDuration}:maxdur={filter.MaxDuration}:isNew={filter.IsNew}";

        // Try distributed cache first
        var cachedBytes = await _distributedCache.GetAsync(cacheKey);
        if (cachedBytes != null)
        {
            try
            {
                var cachedResult = JsonSerializer.Deserialize<CourseFilterResultDto>(cachedBytes);
                if (cachedResult != null)
                {
                    _logger.LogInformation($"Returned filtered courses from distributed cache for key: {cacheKey}");
                    return cachedResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached course filter result");
            }
        }

        var query = _courseRepository.GetQueryable();
        var (paginatedQuery, totalCount) = await _filterPipeline.ExecuteAsync(query, filter);

        var courses = await paginatedQuery.ToListAsync();
        var courseDtos = _mapper.Map<List<CourseDto>>(courses);

        var result = new CourseFilterResultDto
        {
            Courses = courseDtos,
            TotalCount = totalCount,
            CurrentPage = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = _paginator.CalculateTotalPages(totalCount, filter.PageSize),
        };

        // Serialize and set distributed cache (short TTL configurable in CacheSettings)
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(result);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheExpirationMinutes),
            };
            await _distributedCache.SetAsync(cacheKey, bytes, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set distributed cache for course filters");
        }

        _logger.LogInformation($"Successfully retrieved {courseDtos.Count} courses (page {filter.Page} of {result.TotalPages}, total: {totalCount})");

        return result;
    }

    public async Task<CourseDto> GetCourseDetailAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving course detail and incrementing view count for ID: {id}");

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {id} not found.");
            throw new KeyNotFoundException($"Course with ID {id} not found.");
        }

        // Increment view count
        await _courseRepository.IncrementViewCountAsync(id);
        await _unitOfWork.SaveChangesAsync();

        var courseDto = _mapper.Map<CourseDto>(course);
        courseDto.ViewCount = course.ViewCount + 1; // Reflect the incremented count

        _logger.LogInformation($"Successfully retrieved course detail: {id}, ViewCount: {courseDto.ViewCount}");

        return courseDto;
    }

    public List<int> GetPaginationOptions()
    {
        return [9, 18, 36, 72];
    }

    public async Task<CourseImageDto?> GetCourseImageAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving course image for ID: {id}");

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {id} not found.");
            throw new KeyNotFoundException($"Course with ID {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(course.ImageUrl))
        {
            return null;
        }

        var cacheKey = CacheKeys.CourseImage(id);
        if (_memoryCache.TryGetValue(cacheKey, out CourseImageDto? cachedImage) && cachedImage is not null)
        {
            return cachedImage;
        }

        var imageData = await _courseImageStorageService.GetImageAsync(course.ImageUrl);
        if (imageData is null)
        {
            return null;
        }

        var imageDto = new CourseImageDto
        {
            Content = imageData.Value.Content,
            ContentType = imageData.Value.ContentType,
        };

        _memoryCache.Set(cacheKey, imageDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));

        return imageDto;
    }

    public async Task RemoveCourseImageAsync(Guid id)
    {
        _logger.LogInformation($"Removing course image for ID: {id}");

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            _logger.LogError($"Course with ID {id} not found.");
            throw new KeyNotFoundException($"Course with ID {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(course.ImageUrl))
        {
            return;
        }

        await _courseImageStorageService.DeleteImageAsync(course.ImageUrl);

        course.ImageUrl = null;
        _courseRepository.Update(course);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Courses);
        _memoryCache.Remove(CacheKeys.TotalCoursesCount);
        _memoryCache.Remove(CacheKeys.CourseImage(id));
    }

    public List<KeyValuePair<string, string>> GetSortingOptions()
    {
        return
        [
            new("newest", "Newest"),
            new("popularity", "Popular"),
            new("price_asc", "Price: Low to High"),
            new("price_desc", "Price: High to Low"),
            new("rating_desc", "Highest Rated"),
            new("duration_asc", "Duration: Short to Long"),
            new("duration_desc", "Duration: Long to Short"),
            new("title_asc", "Title: A-Z"),
            new("title_desc", "Title: Z-A"),
        ];
    }

    public List<KeyValuePair<CourseLanguage, string>> GetLanguages()
    {
        var cacheKey = CacheKeys.CourseLanguages;
        if (_memoryCache.TryGetValue(cacheKey, out List<KeyValuePair<CourseLanguage, string>>? cachedLanguages) && cachedLanguages != null)
        {
            return cachedLanguages;
        }

        var languages = new List<KeyValuePair<CourseLanguage, string>>
        {
            new(CourseLanguage.Deutsch, "German"),
            new(CourseLanguage.English, "English"),
            new(CourseLanguage.Polski, "Polish"),
            new(CourseLanguage.Italiano, "Italian"),
        };

        _memoryCache.Set(cacheKey, languages, TimeSpan.FromHours(24));
        return languages;
    }

    public List<KeyValuePair<CourseLevel, string>> GetLevels()
    {
        var cacheKey = CacheKeys.CourseLevels;
        if (_memoryCache.TryGetValue(cacheKey, out List<KeyValuePair<CourseLevel, string>>? cachedLevels) && cachedLevels != null)
        {
            return cachedLevels;
        }

        var levels = new List<KeyValuePair<CourseLevel, string>>
        {
            new(CourseLevel.Beginner, "Beginner (A1)"),
            new(CourseLevel.Elementary, "Elementary (A2)"),
            new(CourseLevel.Intermediate, "Intermediate (B1)"),
            new(CourseLevel.UpperIntermediate, "Upper Intermediate (B2)"),
            new(CourseLevel.Advanced, "Advanced (C1)"),
            new(CourseLevel.Proficiency, "Proficiency (C2)"),
        };

        _memoryCache.Set(cacheKey, levels, TimeSpan.FromHours(24));
        return levels;
    }

    public List<KeyValuePair<double, string>> GetRatingOptions()
    {
        var cacheKey = CacheKeys.CourseRatings;
        if (_memoryCache.TryGetValue(cacheKey, out List<KeyValuePair<double, string>>? cachedRatings) && cachedRatings != null)
        {
            return cachedRatings;
        }

        var ratings = new List<KeyValuePair<double, string>>
        {
            new(4.5, "4.5 & up"),
            new(4.0, "4.0 & up"),
            new(3.5, "3.5 & up"),
            new(3.0, "3.0 & up"),
        };

        _memoryCache.Set(cacheKey, ratings, TimeSpan.FromHours(24));
        return ratings;
    }
}
