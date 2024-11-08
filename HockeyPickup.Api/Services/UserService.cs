using HockeyPickup.Api.Models.Domain;

namespace HockeyPickup.Api.Services;

public class UserService : IUserService
{
    public async Task<User> ValidateCredentialsAsync(string username, string password)
    {
        if (username == "test" && password == "test")  // Replace with real validation
        {
            return await Task.FromResult(new User
            {
                Id = "1",
                Email = username
            });
        }

        return null;
    }
}
