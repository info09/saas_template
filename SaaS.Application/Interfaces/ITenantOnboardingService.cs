using SaaS.Application.Common.Models;

namespace SaaS.Application.Interfaces;

public interface ITenantOnboardingService
{
    Task<Result<string>> CreateTenantAsync(string name, string adminEmail, string adminPassword, CancellationToken cancellationToken);
}
