using HockeyPickup.Api.Data.Entities;

public partial class AspNetRole
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;

    public virtual ICollection<AspNetUser> Users { get; set; } = new List<AspNetUser>();
}
