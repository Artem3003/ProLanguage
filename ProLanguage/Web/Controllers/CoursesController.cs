using Application.DTOs.Course;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Courses Management")]
[Authorize]
public class CoursesController(ICourseService courseService, IOrderService orderService) : ControllerBase
{
    private static readonly Counter CourseViewsCounter = Metrics.CreateCounter("prolanguage_courses_viewed_total", "Total number of times a course was viewed.");
    private static readonly Counter FilterSearchesCounter = Metrics.CreateCounter("prolanguage_courses_searched_total", "Total number of filtered course searches.");

    private readonly ICourseService _courseService = courseService;
    private readonly IOrderService _orderService = orderService;

    [HttpPost]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult<Guid>> CreateCourse([FromBody] CreateCourseWithImageDto request)
    {
        request.Course.Image = request.Image;

        var courseId = await _courseService.CreateCourseAsync(request.Course);
        return CreatedAtAction(nameof(GetCourseById), new { id = courseId }, courseId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return Ok(course);
    }

    [HttpGet("{id}/detail")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetCourseDetail(Guid id)
    {
        CourseViewsCounter.Inc();
        var course = await _courseService.GetCourseDetailAsync(id);
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

    [HttpGet("filter")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseFilterResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseFilterResultDto>> GetFilteredCourses([FromQuery] CourseFilterDto filter)
    {
        FilterSearchesCounter.Inc();
        var result = await _courseService.GetFilteredCoursesAsync(filter);
        return Ok(result);
    }

    [HttpGet("filter/options")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseFilterOptionsDto), StatusCodes.Status200OK)]
    public ActionResult<CourseFilterOptionsDto> GetFilterOptions()
    {
        var options = new CourseFilterOptionsDto
        {
            PaginationOptions = _courseService.GetPaginationOptions(),
            SortingOptions = _courseService.GetSortingOptions(),
            Languages = _courseService.GetLanguages(),
            Levels = _courseService.GetLevels(),
            RatingOptions = _courseService.GetRatingOptions(),
        };
        return Ok(options);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAvailableCourses([FromQuery] Guid? excludeLessonId = null)
    {
        var courses = await _courseService.GetAvailableCoursesAsync(excludeLessonId);
        return Ok(courses);
    }

    [HttpPut]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult> UpdateCourse([FromBody] UpdateCourseWithImageDto request)
    {
        request.Course.Image = request.Image;

        await _courseService.UpdateCourseAsync(request.Course);
        return NoContent();
    }

    [HttpGet("{id}/image")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseImage(Guid id)
    {
        var courseImage = await _courseService.GetCourseImageAsync(id);
        return courseImage is null
            ? NotFound()
            : File(courseImage.Content, courseImage.ContentType);
    }

    [HttpDelete("{id}/image")]
    [Authorize(Policy = "ContentManagement")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourseImage(Guid id)
    {
        await _courseService.RemoveCourseImageAsync(id);
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
