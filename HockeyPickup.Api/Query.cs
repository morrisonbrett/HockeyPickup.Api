using HockeyPickup.Api.Data.Models;
using HockeyPickup.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HockeyPickup.Api;

public class UserType : ObjectType<AspNetUser>
{
    protected override void Configure(IObjectTypeDescriptor<AspNetUser> descriptor)
    {
        descriptor.Description("Represents a user in the system");

        descriptor.Field(u => u.Id).Description("The unique identifier for the user");
        descriptor.Field(u => u.UserName).Description("The username of the user");
        descriptor.Field(u => u.Email).Description("The email address of the user");
        descriptor.Field(u => u.FirstName).Description("The user's first name");
        descriptor.Field(u => u.LastName).Description("The user's last name");
        descriptor.Field(u => u.Rating).Description("The user's rating");
        descriptor.Field(u => u.PaymentPreference).Description("The user's preferred payment method");
        descriptor.Field(u => u.NotificationPreference).Type<EnumType<NotificationPreference>>().Description("The user's preferred notification method");
        descriptor.Field(u => u.Preferred).Description("Whether the user is preferred");
        descriptor.Field(u => u.PreferredPlus).Description("Whether the user is preferred plus");

        // Computed field example
        descriptor.Field("fullName").Description("The user's full name").Resolve(context =>
                $"{context.Parent<AspNetUser>().FirstName} {context.Parent<AspNetUser>().LastName}".Trim());
    }
}

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