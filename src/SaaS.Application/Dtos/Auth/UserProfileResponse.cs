namespace SaaS.Application.Dtos.Auth;

public record UserProfileResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string TenantId);
