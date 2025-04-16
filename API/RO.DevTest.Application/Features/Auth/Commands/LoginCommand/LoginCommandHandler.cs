using MediatR;
using RO.DevTest.Application.Contracts.Infrastructure;
using RO.DevTest.Domain.Entities;
using RO.DevTest.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RO.DevTest.Application.Features.Auth.Commands.LoginCommand
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IIdentityAbstractor _identityAbstractor;

        public LoginCommandHandler(IIdentityAbstractor identityAbstractor)
        {
            _identityAbstractor = identityAbstractor;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityAbstractor.FindUserByEmailAsync(request.Username);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuário não encontrado.");
            }

            var signInResult = await _identityAbstractor.PasswordSignInAsync(user, request.Password);
            if (!signInResult.Succeeded)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas.");
            }

            string accessToken = GenerateAccessToken(user);

            var roles = await _identityAbstractor.GetUserRolesAsync(user);


            return new LoginResponse
            {
                AccessToken = accessToken,
                IssuedAt = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddHours(1), // Example expiration
                Roles = roles
            };
        }

        private string GenerateAccessToken(Domain.Entities.User user)
        {
            return Guid.NewGuid().ToString(); // Or something else depending on your architecture
        }
    }
}
