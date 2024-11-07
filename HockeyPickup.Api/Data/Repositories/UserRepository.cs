using HockeyPickup.Api.Data.Context;
using HockeyPickup.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace HockeyPickup.Api.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HockeyPickupContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(HockeyPickupContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetActiveUsersAsync()
    {
        return await _context.AspNetUsers
            .Where(u => u.Active)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Rating = u.Rating,
                IsPreferred = u.Preferred,
                IsPreferredPlus = u.PreferredPlus,
            })
            .ToListAsync();
    }
}
