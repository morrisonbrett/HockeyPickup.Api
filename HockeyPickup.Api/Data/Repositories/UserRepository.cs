using HockeyPickup.Api.Data.Context;
using HockeyPickup.Api.Data.Entities;
using HockeyPickup.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

    public async Task<IEnumerable<UserBasicResponse>> GetBasicUsersAsync()
    {
        return await _context.Users
            .Where(u => u.Active)
            .Select(u => new UserBasicResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Preferred = u.Preferred,
                PreferredPlus = u.PreferredPlus,
                Active = u.Active,
                LockerRoom13 = u.LockerRoom13,
                EmergencyName = u.EmergencyName,
                EmergencyPhone = u.EmergencyPhone,
                MobileLast4 = u.MobileLast4,
                VenmoAccount = u.VenmoAccount,
                PayPalEmail = u.PayPalEmail,
                NotificationPreference = (NotificationPreference) u.NotificationPreference,
                DateCreated = u.DateCreated,
                Roles = u.Roles.Where(role => role.Name != null).Select(role => role.Name!).ToArray(),
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDetailedResponse>> GetDetailedUsersAsync()
    {
        return await _context.Users
            .Where(u => u.Active)
            .Select(u => new UserDetailedResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Rating = u.Rating,
                Preferred = u.Preferred,
                PreferredPlus = u.PreferredPlus,
                Active = u.Active,
                LockerRoom13 = u.LockerRoom13,
                EmergencyName = u.EmergencyName,
                EmergencyPhone = u.EmergencyPhone,
                MobileLast4 = u.MobileLast4,
                VenmoAccount = u.VenmoAccount,
                PayPalEmail = u.PayPalEmail,
                NotificationPreference = (NotificationPreference) u.NotificationPreference,
                DateCreated = u.DateCreated,
                Roles = u.Roles.Where(role => role.Name != null).Select(role => role.Name!).ToArray(),
            })
            .ToListAsync();
    }

    public async Task<UserBasicResponse> GetUserAsync(string userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserBasicResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Preferred = u.Preferred,
                PreferredPlus = u.PreferredPlus,
                Active = u.Active,
                LockerRoom13 = u.LockerRoom13,
                EmergencyName = u.EmergencyName,
                EmergencyPhone = u.EmergencyPhone,
                MobileLast4 = u.MobileLast4,
                VenmoAccount = u.VenmoAccount,
                PayPalEmail = u.PayPalEmail,
                NotificationPreference = (NotificationPreference) u.NotificationPreference,
                DateCreated = u.DateCreated,
                Roles = u.Roles.Where(role => role.Name != null).Select(role => role.Name!).ToArray(),
            })
            .FirstOrDefaultAsync();
    }
}
