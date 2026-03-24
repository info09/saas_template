using Microsoft.AspNetCore.Identity;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;
using SaaS.Infrastructure.Identity;

namespace SaaS.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(Result Result, string UserId)> AuthenticateAsync(string email, string password, string tenantId)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return (Result.Failure("Invalid credentials."), string.Empty);
        }

        var result = await _userManager.CheckPasswordAsync(user, password);

        if (!result)
        {
            return (Result.Failure("Invalid credentials."), string.Empty);
        }

        return (Result.Success(), user.Id);
    }

    public async Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result.Failure(result.Errors.Select(e => e.Description).ToArray());
        }

        return Result.Success();
    }
}
