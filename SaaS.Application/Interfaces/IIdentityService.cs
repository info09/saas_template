using SaaS.Application.Common;

namespace SaaS.Application.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, string UserId)> AuthenticateAsync(string email, string password, string tenantId);
    Task<Result> CreateUserAsync(string email, string password, string firstName, string lastName);
}
