namespace Application.DTOs.Course;

public class CourseImageDto
{
    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = "application/octet-stream";
}
