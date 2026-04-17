using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public partial class CourseImageStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<AzureBlobStorageSettings> settings,
    ILogger<CourseImageStorageService> logger) : ICourseImageStorageService
{
    private const int MaxImageBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private readonly AzureBlobStorageSettings _settings = settings.Value;
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly ILogger<CourseImageStorageService> _logger = logger;

    public async Task<string> UploadCourseImageAsync(Guid courseId, string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new InvalidOperationException("Image payload is required.");
        }

        var (contentType, imageBytes) = ParseAndValidate(base64Image);
        var containerClient = await GetContainerClientAsync();
        var imageHash = Convert.ToHexString(SHA256.HashData(imageBytes)).ToLowerInvariant();

        var extension = contentType switch
        {
            "image/png" => "png",
            "image/webp" => "webp",
            _ => "jpg",
        };

        var blobName = $"courses/{imageHash}.{extension}";
        var blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            _logger.LogInformation("Reusing existing course image blob {BlobName} for course {CourseId}", blobName, courseId);
            return blobClient.Uri.ToString();
        }

        await using var stream = new MemoryStream(imageBytes);
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.ToString();
    }

    public async Task<(byte[] Content, string ContentType)?> GetImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var containerClient = await GetContainerClientAsync();
        var blobName = ExtractBlobName(imageUrl);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        var contentType = response.Value.Details.ContentType;

        return (response.Value.Content.ToArray(), string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var containerClient = await GetContainerClientAsync();
        var blobName = ExtractBlobName(imageUrl);
        var blobClient = containerClient.GetBlobClient(blobName);

        try
        {
            await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to delete blob image {ImageUrl}", imageUrl);
            throw;
        }
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
        {
            throw new InvalidOperationException("AzureBlobStorage:ConnectionString is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ContainerName))
        {
            throw new InvalidOperationException("AzureBlobStorage:ContainerName is not configured.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        return containerClient;
    }

    private static (string ContentType, byte[] Content) ParseAndValidate(string image)
    {
        var match = DataUriRegex().Match(image.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException("Image must be a valid base64 data URI.");
        }

        var contentType = match.Groups[1].Value.ToLowerInvariant();
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new InvalidOperationException("Unsupported image format. Allowed formats: jpg, jpeg, png, webp.");
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(match.Groups[2].Value);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Image base64 payload is invalid.");
        }

        _ = imageBytes.Length <= MaxImageBytes
            ? true
            : throw new InvalidOperationException("Image size exceeds 5 MB limit.");

        return (contentType, imageBytes);
    }

    private string ExtractBlobName(string imageUrl)
    {
        var uri = new Uri(imageUrl, UriKind.Absolute);

        var absolutePath = uri.AbsolutePath.Trim('/');
        var expectedPrefix = $"{_settings.ContainerName}/";

        _ = absolutePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            ? true
            : throw new InvalidOperationException("Image URL does not match configured Azure Blob container.");

        return absolutePath[expectedPrefix.Length..];
    }

    [GeneratedRegex("^data:(image\\/(?:jpeg|png|webp));base64,([A-Za-z0-9+/=\\r\\n]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex DataUriRegex();
}
