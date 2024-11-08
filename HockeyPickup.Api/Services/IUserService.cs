using HockeyPickup.Api.Models.Domain;

namespace HockeyPickup.Api.Services
{
    public interface IUserService
    {
        Task<User> ValidateCredentialsAsync(string username, string password);
    }
}