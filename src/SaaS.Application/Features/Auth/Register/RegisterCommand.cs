using MediatR;
using SaaS.Application.Common.Models;

namespace SaaS.Application.Features.Auth.Register;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName, string TenantId) : IRequest<Result>;
