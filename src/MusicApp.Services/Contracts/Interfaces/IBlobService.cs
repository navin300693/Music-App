namespace MusicApp.Services.Contracts.Interfaces;

public interface IBlobService
{
    /// <summary>
    /// Uploads a stream to a specific blob container.
    /// </summary>
    /// <returns>The unique name/path of the stored blob.</returns>
    Task<string> UploadFileAsync(string containerName, string blobName, Stream stream);

    /// <summary>
    /// Generates a time-limited secure SAS URI for playback.
    /// </summary>
    Task<string> GenerateSecureLinkAsync(string containerName, string subFolder, string blobName);
}