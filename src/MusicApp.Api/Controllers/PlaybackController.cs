namespace MusicApp.Api.Controllers;

// [Single Responsibility Principle - SRP]: This controller's only
// responsibility is handling HTTP requests and returning proper Status Codes.
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
// [Dependency Injection Pattern]: We inject IPlaybackService.
// The controller is "Open for Extension" (OCP) because we could
// swap the service implementation without changing this code.
public class PlaybackController(IPlaybackService playbackService, ILogger<PlaybackController> logger) : ControllerBase
{
    /// <summary>Returns streaming metadata for a song, or 404 if not found or access is denied.</summary>
    [HttpGet("stream/{id:guid}")]
    [ProducesResponseType(typeof(PlaybackStreamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaybackStreamResponse>> GetStream(Guid id)
    {
        logger.LogInformation("Fetching stream for song ID {SongId}", id);
        var result = await playbackService.GetStreamAsync(id);

        if (result == null)
        {
            logger.LogWarning("Stream not found or access denied for song ID {SongId}", id);
            return NotFound();
        }

        logger.LogInformation("Stream returned for song ID {SongId}", id);
        return Ok(result);
    }

    /// <summary>Attempts to skip the current track based on the user's playback policy.</summary>
    [HttpPost("skip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Skip()
    {
        logger.LogInformation("Skip requested");
        var success = await playbackService.AttemptSkipAsync();

        if (!success)
        {
            logger.LogWarning("Skip denied by playback policy");
            return Forbid();
        }

        logger.LogInformation("Skip succeeded");
        return Ok();
    }
}
