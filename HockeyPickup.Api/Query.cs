using HockeyPickup.Api.Data.Repositories;
using HockeyPickup.Api.Models.Responses;

namespace HockeyPickup.Api.GraphQL;

public class Query
{
    [GraphQLDescription("Retrieves a list of all active users")]
    public async Task<IEnumerable<UserResponse>> Users([Service] IUserRepository userRepository)
    {
        return await userRepository.GetActiveUsersAsync();
    }
}