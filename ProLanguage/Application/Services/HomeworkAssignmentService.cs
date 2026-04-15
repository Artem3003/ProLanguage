using Application.Constants;
using Application.DTOs.HomeworkAssignment;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class HomeworkAssignmentService(IUnitOfWork unitOfWork, IHomeworkAssignmentRepository assignmentRepository, IMapper mapper, IOptions<CacheSettings> cacheSettings, IMemoryCache memoryCache, ILogger<HomeworkAssignmentService> logger) : IHomeworkAssignmentService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHomeworkAssignmentRepository _assignmentRepository = assignmentRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<HomeworkAssignmentService> _logger = logger;
    private readonly int _cacheExpirationMinutes = cacheSettings.Value.DefaultExpirationMinutes;

    public async Task<Guid> CreateAssignmentAsync(CreateHomeworkAssignmentDto dto)
    {
        _logger.LogInformation($"Starting homework assignment creation for homework ID: {dto.HomeworkId}");

        var assignment = _mapper.Map<HomeworkAssignment>(dto);
        if (assignment is null)
        {
            _logger.LogError($"Homework assignment mapping failed - unable to map request to HomeworkAssignment entity for homework ID: {dto.HomeworkId}");
            throw new InvalidOperationException("Homework assignment cannot be null.");
        }

        _logger.LogDebug($"Mapped homework assignment DTO to entity for homework ID: {dto.HomeworkId}");

        await _assignmentRepository.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        var assignmentDto = _mapper.Map<HomeworkAssignmentDto>(assignment);

        _memoryCache.Set(CacheKeys.HomeworkAssignments, assignmentDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Homework assignment cached for ID: {assignment.Id}");

        _logger.LogInformation($"Successfully created homework assignment with ID: {assignment.Id}, Homework ID: {assignment.HomeworkId}");

        return assignment.Id;
    }

    public async Task<HomeworkAssignmentDto> GetAssignmentByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving homework assignment by ID: {id}");

        if (_memoryCache.TryGetValue(CacheKeys.HomeworkAssignments, out HomeworkAssignmentDto? cachedAssignment))
        {
            _logger.LogInformation($"Homework assignment found in cache for ID: {id}");
            return cachedAssignment;
        }

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null)
        {
            _logger.LogError($"Assignment with ID {id} not found.");
            throw new KeyNotFoundException($"Assignment with ID {id} not found.");
        }

        _logger.LogDebug($"Homework assignment retrieved from repository for ID: {id}");

        var assignmentDto = _mapper.Map<HomeworkAssignmentDto>(assignment);
        _memoryCache.Set(CacheKeys.HomeworkAssignments, assignmentDto, TimeSpan.FromMinutes(_cacheExpirationMinutes));
        _logger.LogDebug($"Homework assignment cached for ID: {id}");

        _logger.LogInformation($"Successfully retrieved homework assignment: {id}");

        return assignmentDto;
    }

    public async Task<IEnumerable<HomeworkAssignmentDto>> GetAllAssignmentsAsync()
    {
        _logger.LogInformation($"Retrieving all homework assignments");

        var assignments = await _assignmentRepository.GetAllAsync();
        if (assignments is null || !assignments.Any())
        {
            _logger.LogError($"No homework assignments found.");
            throw new KeyNotFoundException("No homework assignments found.");
        }

        var result = _mapper.Map<IEnumerable<HomeworkAssignmentDto>>(assignments) ?? [];

        _logger.LogInformation($"Successfully retrieved all homework assignments");

        return result;
    }

    public async Task SubmitAssignmentAsync(Guid id, SubmitHomeworkAssignmentDto dto)
    {
        _logger.LogInformation($"Starting homework assignment submission for ID: {id}");

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null)
        {
            _logger.LogError($"Assignment with ID {id} not found.");
            throw new KeyNotFoundException($"Assignment with ID {id} not found.");
        }

        _logger.LogDebug($"Found existing homework assignment: {assignment.Id}, Status: {assignment.Status}");

        _mapper.Map(dto, assignment);
        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.HomeworkAssignments);
        _logger.LogDebug($"Cleared homework assignments cache after submission");

        _logger.LogInformation($"Successfully submitted homework assignment: {assignment.Id}");
    }

    public async Task GradeAssignmentAsync(Guid id, GradeHomeworkAssignmentDto dto)
    {
        _logger.LogInformation($"Starting homework assignment grading for ID: {id}");

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null)
        {
            _logger.LogError($"Assignment with ID {id} not found.");
            throw new KeyNotFoundException($"Assignment with ID {id} not found.");
        }

        _logger.LogDebug($"Found existing homework assignment: {assignment.Id}, Current Grade: {assignment.Grade}");

        _mapper.Map(dto, assignment);
        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.HomeworkAssignments);
        _logger.LogDebug($"Cleared homework assignments cache after grading");

        _logger.LogInformation($"Successfully graded homework assignment: {assignment.Id}, New Grade: {assignment.Grade}");
    }

    public async Task DeleteAssignmentAsync(Guid id)
    {
        _logger.LogInformation($"Starting homework assignment deletion for ID: {id}");

        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment is null)
        {
            _logger.LogError($"Assignment with ID {id} not found.");
            throw new KeyNotFoundException($"Assignment with ID {id} not found.");
        }

        _assignmentRepository.Delete(assignment);
        await _unitOfWork.SaveChangesAsync();

        _memoryCache.Remove(CacheKeys.HomeworkAssignments);
        _logger.LogDebug($"Cleared homework assignments cache after deletion");

        _logger.LogInformation($"Successfully deleted homework assignment: {assignment.Id}");
    }
}
