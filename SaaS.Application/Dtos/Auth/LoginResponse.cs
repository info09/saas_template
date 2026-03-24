namespace SaaS.Application.Dtos.Auth;

public record LoginResponse(bool Succeeded, string Token, IEnumerable<string>? Errors);
