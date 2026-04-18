namespace MusicApp.Api.Controllers;

// [Single Responsibility Principle - SRP]: This controller's only
// responsibility is handling HTTP requests and returning proper Status Codes.
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
// [Dependency Injection Pattern]: We inject ISongService.
// The controller is "Open for Extension" (OCP) because we could
// swap the service implementation without changing this code.
public class SongsController(ISongService songService, ILogger<SongsController> logger) : ControllerBase
{
    /// <summary>Returns all songs.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SongResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SongResponse>>> GetAll()
    {
        logger.LogInformation("Fetching all songs");
        var results = await songService.GetAllSongsAsync();
        var songs = results.ToList();
        logger.LogInformation("Returned {Count} songs", songs.Count);
        return Ok(songs);
    }

    /// <summary>Creates a new song from a multipart/form-data request.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateSongRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateSongRequest>> Create([FromForm] CreateSongRequest request)
    {
        logger.LogInformation("Creating song: {Title}", request.Title);
        var songId = await songService.CreateSongAsync(request);
        logger.LogInformation("Song created with ID {SongId}", songId);

        // [REST Best Practice]: Return 201 Created with the location of the new resource.
        return CreatedAtAction(nameof(GetById), new { version = HttpContext.GetRequestedApiVersion()!.ToString(), id = songId }, request);
    }

    /// <summary>Returns a single song by its ID, or 404 if not found.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SongResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SongResponse>> GetById(Guid id)
    {
        logger.LogInformation("Fetching song with ID {SongId}", id);
        var song = await songService.GetSongByIdAsync(id);

        if (song == null)
        {
            logger.LogWarning("Song with ID {SongId} not found", id);
            return NotFound();
        }

        // [Liskov Substitution Principle - LSP]: The controller works correctly
        // regardless of which specific ISongService implementation is provided.
        return Ok(song);
    }

    /// <summary>Creates multiple songs in a single request.</summary>
    [HttpPost("bulk")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBulk([FromForm] IEnumerable<CreateSongRequest> requests)
    {
        if (requests == null || !requests.Any())
        {
            logger.LogWarning("Bulk create called with empty song list");
            ModelState.AddModelError(nameof(requests), "Song list cannot be empty.");
            return ValidationProblem();
        }

        var requestList = requests.ToList();
        logger.LogInformation("Bulk creating {Count} songs", requestList.Count);
        var ids = await songService.CreateMultipleSongsAsync(requestList);
        var createdIds = ids.ToList();
        logger.LogInformation("Bulk create succeeded; created {Count} songs", createdIds.Count);

        // [REST Best Practice]: Return 201 Created — Location header is optional per RFC 7231,
        // so we omit it and carry all created IDs in the response body instead.
        return StatusCode(StatusCodes.Status201Created, new { CreatedIds = createdIds, createdIds.Count });
    }
}