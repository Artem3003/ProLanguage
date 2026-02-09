using Application.DTOs.Course;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Courses Management")]
[Authorize]
public class CoursesController(ICourseService courseService, IOrderService orderService) : ControllerBase
{
    private readonly ICourseService _courseService = courseService;
    private readonly IOrderService _orderService = orderService;

    [HttpPost]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult<Guid>> CreateCourse([FromBody] CreateCourseDto request)
    {
        var courseId = await _courseService.CreateCourseAsync(request);
        return CreatedAtAction(nameof(GetCourseById), new { id = courseId }, courseId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return Ok(course);
    }

    [HttpGet("by-title/{title}")]
    public async Task<ActionResult<CourseDto>> GetCourseByTitle(string title)
    {
        var course = await _courseService.GetCourseByTitleAsync(title);
        return course is null ? (ActionResult<CourseDto>)NotFound($"Course with title '{title}' not found.") : (ActionResult<CourseDto>)Ok(course);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAvailableCourses([FromQuery] Guid? excludeLessonId = null)
    {
        var courses = await _courseService.GetAvailableCoursesAsync(excludeLessonId);
        return Ok(courses);
    }

    [HttpPut]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult> UpdateCourse([FromBody] UpdateCourseDto request)
    {
        await _courseService.UpdateCourseAsync(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        await _courseService.DeleteCourseAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/buy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart(Guid id)
    {
        try
        {
            await _orderService.AddToCartAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
