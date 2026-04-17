namespace Infrastructure.Settings;

public class AzureBlobStorageSettings
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "course-images";
}
