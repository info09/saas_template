using MediatR;
using SaaS.Application.Common.Models;
using SaaS.Application.Interfaces;

namespace SaaS.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly IIdentityService _identityService;

        public RegisterCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.CreateUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
        }
    }
}
