using Application.Constants;
using Application.DTOs.Lesson;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class LessonService(IUnitOfWork unitOfWork, ILessonRepository lessonRepository, IMapper mapper, IOptions<CacheSettings> cacheSettings, IMemoryCache memoryCache, ILogger<LessonService> logger) : ILessonService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILessonRepository _lessonRepository = lessonRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<LessonService> _logger = logger;
    private readonly int _cacheExpirationMinutes = cacheSettings.Value.DefaultExpirationMinutes;

    public async Task<Guid> CreateLessonAsync(CreateLessonDto dto)
    {
        _logger.LogInformation($"Starting lesson creation for title: {dto.Title}");

        var lesson = _mapper.Map<Lesson>(dto);
        if (lesson is null)
        {
            _logger.LogError($"Lesson mapping failed - unable to map request to Lesson entity for title: {dto.Title}");
            throw new InvalidOperationException("Lesson cannot be null.");
        }

        _logger.LogDebug($"Mapped lesson DTO to entity for title: {dto.Title}");

        await _lessonRepository.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        var lessonDto = _mapper.Map<LessonDto>(lesson);
        _memoryCache.Set(CacheKeys.Lessons, lessonDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _memoryCache.Remove(CacheKeys.TotalLessonsCount);
        _logger.LogDebug($"Cleared lessons cache after creating lesson");

        _logger.LogInformation($"Successfully created lesson with ID: {lesson.Id}, Title: {lesson.Title}");

        return lesson.Id;
    }

    public async Task<LessonDto> GetLessonByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving lesson by ID: {id}");

        if (_memoryCache.TryGetValue(CacheKeys.Lessons, out LessonDto? cachedLesson))
        {
            _logger.LogInformation($"Lesson found in cache for ID: {id}");
            return cachedLesson;
        }

        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson is null)
        {
            _logger.LogError($"Lesson with ID {id} not found.");
            throw new KeyNotFoundException($"Lesson with ID {id} not found.");
        }

        _logger.LogDebug($"Lesson retrieved from repository for ID: {id}");

        var lessonDto = _mapper.Map<LessonDto>(lesson);
        _memoryCache.Set(CacheKeys.Lessons, lessonDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Lesson cached for ID: {id}");

        _logger.LogInformation($"Successfully retrieved lesson: {id}");

        return lessonDto;
    }

    public async Task<IEnumerable<LessonDto>> GetAllLessonsAsync()
    {
        _logger.LogInformation($"Retrieving all lessons");

        var lessons = await _lessonRepository.GetAllAsync();
        if (lessons is null || !lessons.Any())
        {
            _logger.LogError($"No lessons found.");
            throw new KeyNotFoundException("No lessons found.");
        }

        var result = _mapper.Map<IEnumerable<LessonDto>>(lessons) ?? [];

        _logger.LogInformation($"Successfully retrieved all lessons");

        return result;
    }

    public async Task<int> GetTotalLessonsCountAsync()
    {
        _logger.LogInformation($"Retrieving lessons count");

        if (_memoryCache.TryGetValue(CacheKeys.TotalLessonsCount, out int cachedCount))
        {
            _logger.LogDebug($"Lessons count found in cache: {cachedCount}");
            return cachedCount;
        }

        var totalLessons = await _lessonRepository.CountLessonsAsync();

        _memoryCache.Set(CacheKeys.TotalLessonsCount, totalLessons, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Lessons count cached: {totalLessons}");

        _logger.LogInformation($"Successfully retrieved lessons count: {totalLessons}");

        return totalLessons;
    }

    public async Task UpdateLessonAsync(UpdateLessonDto dto)
    {
        _logger.LogInformation($"Starting lesson update for ID: {dto.Id}");

        var lesson = await _lessonRepository.GetByIdAsync(dto.Id);
        if (lesson is null)
        {
            _logger.LogError($"Lesson with ID {dto.Id} not found.");
            throw new KeyNotFoundException($"Lesson with ID {dto.Id} not found.");
        }

        _logger.LogDebug($"Found existing lesson: {lesson.Id}, Title: {lesson.Title}");

        _mapper.Map(dto, lesson);
        _lessonRepository.Update(lesson);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Lessons);
        _memoryCache.Remove(CacheKeys.TotalLessonsCount);
        _logger.LogDebug($"Cleared lessons cache after updating lesson");

        _logger.LogInformation($"Successfully updated lesson: {lesson.Id}, Title: {lesson.Title}");
    }

    public async Task DeleteLessonAsync(Guid id)
    {
        _logger.LogInformation($"Starting lesson deletion for ID: {id}");

        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson is null)
        {
            _logger.LogError($"Lesson with ID {id} not found.");
            throw new KeyNotFoundException($"Lesson with ID {id} not found.");
        }

        _lessonRepository.Delete(lesson);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Lessons);
        _memoryCache.Remove(CacheKeys.TotalLessonsCount);
        _memoryCache.Remove("CalendarEvents_All");
        _logger.LogDebug($"Cleared lessons and calendar events cache after deleting lesson");

        _logger.LogInformation($"Successfully deleted lesson: {lesson.Id}");
    }
}
