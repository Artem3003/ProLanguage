namespace Application.Interfaces;

public interface ICourseImageStorageService
{
    Task<string> UploadCourseImageAsync(Guid courseId, string base64Image);

    Task<(byte[] Content, string ContentType)?> GetImageAsync(string imageUrl);

    Task DeleteImageAsync(string imageUrl);
}
