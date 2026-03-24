namespace SaaS.Application.Dtos.Auth;

public record LoginRequest(string Email, string Password, string TenantId);
