using Microsoft.AspNetCore.Mvc;
using Yellowtail.API.Contracts;
using Yellowtail.Services.Contracts;

namespace Yellowtail.API.Controllers;

/// <summary>
/// Exposes read-only access to the global sports catalog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SportsController : ControllerBase
{
    /// <summary>
    /// The service used to read sports.
    /// </summary>
    private readonly ISportService _sportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SportsController"/> class.
    /// </summary>
    /// <param name="sportService">The service used to read sports.</param>
    public SportsController(ISportService sportService)
    {
        _sportService = sportService;
    }

    /// <summary>
    /// Gets all sports in the catalog.
    /// </summary>
    /// <returns>The full list of sports.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SportResponse>>> GetAll()
    {
        var sports = await _sportService.GetAllAsync();
        return Ok(sports.Select(SportResponse.FromEntity).ToList());
    }
}
