using HockeyPickup.Api.Data;
using HockeyPickup.Api.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyPickup.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly HockeyPickupContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(HockeyPickupContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all active users
    /// </summary>
    /// <returns>List of users with basic information</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        try
        {
            var users = await _context.AspNetUsers
                .Where(u => u.Active)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Rating = u.Rating,
                    PaymentPreference = u.PaymentPreference,
                    NotificationPreference = (NotificationPreference)u.NotificationPreference,
                    IsPreferred = u.Preferred,
                    IsPreferredPlus = u.PreferredPlus
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }
}

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal Rating { get; set; }
    public int PaymentPreference { get; set; }
    public NotificationPreference NotificationPreference { get; set; }
    public bool IsPreferred { get; set; }
    public bool IsPreferredPlus { get; set; }
}
