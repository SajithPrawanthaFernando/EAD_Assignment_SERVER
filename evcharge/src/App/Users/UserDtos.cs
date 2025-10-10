// UserDtos.cs
using Domain.Users;

namespace App.Users;

public sealed record UserView(
    string Id,
    string Email,
    bool Active,
    string[] Roles,
    string? OwnerNic
);

public static class UserMap
{
    public static UserView ToView(User u) =>
        new(u.Id, u.Email, u.Active, u.Roles.Select(r => r.ToString()).ToArray(), u.OwnerNic);
}