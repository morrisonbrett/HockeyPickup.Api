using HockeyPickup.Api.Models.Responses;
using HockeyPickup.Api.Data.Repositories;

namespace HockeyPickup.Api.Data.GraphQL;

public class Query
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<Query> _logger;

    public Query(IHttpContextAccessor httpContextAccessor, ILogger<Query> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    [GraphQLDescription("Retrieves a list of active users.")]
    [GraphQLType(typeof(IEnumerable<UserDetailedResponse>))]
    [GraphQLName("UsersEx")]
    public async Task<IEnumerable<UserDetailedResponse>> UsersEx([Service] IUserRepository userRepository)
    {
        var detailedUsers = await userRepository.GetDetailedUsersAsync();
        return detailedUsers;
    }

    [GraphQLDescription("Retrieves a list of LockerRoom13 status for each upcoming session.")]
    [GraphQLType(typeof(IEnumerable<LockerRoom13Response>))]
    [GraphQLName("LockerRoom13")]
    public async Task<IEnumerable<LockerRoom13Response>> LockerRoom13([Service] IUserRepository userRepository)
    {
        var lockerRoom13Response = await userRepository.GetLockerRoom13SessionsAsync();
        return lockerRoom13Response;
    }
}
