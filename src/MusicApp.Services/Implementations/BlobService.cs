namespace MusicApp.Services.Implementations;

public class BlobService(BlobServiceClient blobClient, ILogger<BlobService> logger) : IBlobService
{
    /// <summary>
    /// Uploads the stream to three quality tiers (high, standard, low) under the given container.
    /// Returns the blob name used as the stored path.
    /// </summary>
    public async Task<string> UploadFileAsync(string containerName, string blobName, Stream stream)
    {
        logger.LogInformation("Uploading blob {BlobName} to container {Container}", blobName, containerName);

        var container = blobClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        var blob1 = container.GetBlobClient($"high/{blobName}");
        var blob2 = container.GetBlobClient($"standard/{blobName}");
        var blob3 = container.GetBlobClient($"low/{blobName}");

        // Resets the stream position before each upload to support multiple writes
        async Task ResetAndUpload(BlobClient client)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            await client.UploadAsync(stream, overwrite: true);
        }

        await ResetAndUpload(blob1);
        logger.LogInformation("Uploaded high-quality tier for {BlobName}", blobName);

        await ResetAndUpload(blob2);
        logger.LogInformation("Uploaded standard-quality tier for {BlobName}", blobName);

        await ResetAndUpload(blob3);
        logger.LogInformation("Uploaded low-quality tier for {BlobName}", blobName);

        return blobName;
    }

    /// <summary>
    /// Generates a time-limited SAS URL for the given blob, swapped to the CDN origin.
    /// </summary>
    public async Task<string> GenerateSecureLinkAsync(string containerName, string subFolder, string blobName)
    {
        var fullPath = $"{subFolder}/{blobName}";
        logger.LogInformation("Generating secure link for {FullPath} in container {Container}", fullPath, containerName);

        var blob = blobClient.GetBlobContainerClient(containerName).GetBlobClient(fullPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fullPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = blob.GenerateSasUri(sasBuilder).Query;

        // Swap the storage origin for the CDN host so clients hit the edge cache
        var cdnUrl = $"https://cdn.musicapp.com/{containerName}/{fullPath}{sasToken}";
        logger.LogInformation("Secure CDN link generated for {FullPath}", fullPath);

        return await Task.FromResult(cdnUrl);
    }
}
