using HockeyPickup.Api.Data.Models;
using HockeyPickup.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HockeyPickup.Api;

public class Query
{
    private readonly ILogger<Query> _logger;

    public Query(ILogger<Query> logger)
    {
        _logger = logger;
    }

    [GraphQLDescription("Retrieves a list of all active users")]
    public async Task<IEnumerable<AspNetUser>> GetUsers([Service] HockeyPickupContext context)
    {
        try
        {
            return await context.AspNetUsers
                .Where(u => u.Active)
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users through GraphQL");
            throw;
        }
    }
}