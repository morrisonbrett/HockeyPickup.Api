using HockeyPickup.Api.Controllers;
using HockeyPickup.Api.Models.Responses;

namespace HockeyPickup.Api.Data.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserResponse>> GetActiveUsersAsync();
}