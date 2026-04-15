using Application.Constants;
using Application.DTOs.Homework;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class HomeworkService(IUnitOfWork unitOfWork, IHomeworkRepository homeworkRepository, IMapper mapper, IOptions<CacheSettings> cacheSettings, IMemoryCache memoryCache, ILogger<HomeworkService> logger) : IHomeworkService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHomeworkRepository _homeworkRepository = homeworkRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<HomeworkService> _logger = logger;
    private readonly int _cacheExpirationMinutes = cacheSettings.Value.DefaultExpirationMinutes;

    public async Task<Guid> CreateHomeworkAsync(CreateHomeworkDto dto)
    {
        _logger.LogInformation($"Starting homework creation for title: {dto.Title}");

        var homework = _mapper.Map<Homework>(dto);
        if (homework is null)
        {
            _logger.LogError($"Homework mapping failed - unable to map request to Homework entity for title: {dto.Title}");
            throw new InvalidOperationException("Homework cannot be null.");
        }

        _logger.LogDebug($"Mapped homework DTO to entity for title: {dto.Title}");

        await _homeworkRepository.AddAsync(homework);
        await _unitOfWork.SaveChangesAsync();

        var homeworkDto = _mapper.Map<HomeworkDto>(homework);
        _memoryCache.Set(CacheKeys.Homework, homeworkDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Homework cached for title: {dto.Title}");

        _logger.LogInformation($"Successfully created homework with ID: {homework.Id}, Title: {homework.Title}");

        return homework.Id;
    }

    public async Task<HomeworkDto> GetHomeworkByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving homework by ID: {id}");

        if (_memoryCache.TryGetValue(CacheKeys.Homework, out HomeworkDto? cachedHomework))
        {
            _logger.LogInformation($"Homework found in cache for ID: {id}");
            return cachedHomework;
        }

        var homework = await _homeworkRepository.GetByIdAsync(id);
        if (homework is null)
        {
            _logger.LogError($"Homework with ID {id} not found.");
            throw new KeyNotFoundException($"Homework with ID {id} not found.");
        }

        _logger.LogDebug($"Homework retrieved from repository for ID: {id}");

        var homeworkDto = _mapper.Map<HomeworkDto>(homework);
        _memoryCache.Set(CacheKeys.Homework, homeworkDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Homework cached for ID: {id}");

        _logger.LogInformation($"Successfully retrieved homework: {id}");

        return homeworkDto;
    }

    public async Task<IEnumerable<HomeworkDto>> GetAllHomeworksAsync()
    {
        _logger.LogInformation($"Retrieving all homeworks");

        var homeworks = await _homeworkRepository.GetAllAsync();
        if (homeworks is null || !homeworks.Any())
        {
            _logger.LogError($"No homeworks found.");
            throw new KeyNotFoundException("No homeworks found.");
        }

        var result = _mapper.Map<IEnumerable<HomeworkDto>>(homeworks) ?? [];

        _logger.LogInformation($"Successfully retrieved all homeworks");

        return result;
    }

    public async Task UpdateHomeworkAsync(UpdateHomeworkDto dto)
    {
        _logger.LogInformation($"Starting homework update for ID: {dto.Id}");

        var homework = await _homeworkRepository.GetByIdAsync(dto.Id);
        if (homework is null)
        {
            _logger.LogError($"Homework with ID {dto.Id} not found.");
            throw new KeyNotFoundException($"Homework with ID {dto.Id} not found.");
        }

        _logger.LogDebug($"Found existing homework: {homework.Id}, Title: {homework.Title}");

        _mapper.Map(dto, homework);
        _homeworkRepository.Update(homework);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Homework);
        _logger.LogDebug($"Cleared homework cache after updating homework");

        _logger.LogInformation($"Successfully updated homework: {homework.Id}, Title: {homework.Title}");
    }

    public async Task DeleteHomeworkAsync(Guid id)
    {
        _logger.LogInformation($"Starting homework deletion for ID: {id}");

        var homework = await _homeworkRepository.GetByIdAsync(id);
        if (homework is null)
        {
            _logger.LogError($"Homework with ID {id} not found.");
            throw new KeyNotFoundException($"Homework with ID {id} not found.");
        }

        _homeworkRepository.Delete(homework);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.Homework);
        _logger.LogDebug($"Cleared homework cache after deleting homework");

        _logger.LogInformation($"Successfully deleted homework: {homework.Id}");
    }
}
