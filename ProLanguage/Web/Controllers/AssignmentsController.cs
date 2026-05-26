using Application.DTOs.HomeworkAssignment;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Homework Assignments")]
[Authorize]
public class AssignmentsController(IHomeworkAssignmentService assignmentService) : ControllerBase
{
    private readonly IHomeworkAssignmentService _assignmentService = assignmentService;

    [HttpPost]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult<Guid>> CreateAssignment([FromBody] CreateHomeworkAssignmentDto request)
    {
        var assignmentId = await _assignmentService.CreateAssignmentAsync(request);
        return CreatedAtAction(nameof(GetAssignmentById), new { id = assignmentId }, assignmentId);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<HomeworkAssignmentDto>> GetAssignmentById(Guid id)
    {
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
        return Ok(assignment);
    }

    [HttpGet]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<IEnumerable<HomeworkAssignmentDto>>> GetAllAssignments()
    {
        var assignments = await _assignmentService.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpPut("{id}/submit")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult> SubmitAssignment(Guid id, [FromBody] SubmitHomeworkAssignmentDto request)
    {
        await _assignmentService.SubmitAssignmentAsync(id, request);
        return NoContent();
    }

    [HttpPut("{id}/grade")]
    [Authorize(Policy = "GradingAccess")]
    public async Task<ActionResult> GradeAssignment(Guid id, [FromBody] GradeHomeworkAssignmentDto request)
    {
        await _assignmentService.GradeAssignmentAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult> DeleteAssignment(Guid id)
    {
        await _assignmentService.DeleteAssignmentAsync(id);
        return NoContent();
    }
}