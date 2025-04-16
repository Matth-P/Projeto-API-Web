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
    /// <summary>
    /// Handler para o comando de Login.
    /// Este handler é responsável por processar o login do usuário e retornar os dados de autenticação.
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IIdentityAbstractor _identityAbstractor;

        /// <summary>
        /// Construtor para o LoginCommandHandler.
        /// </summary>
        /// <param name="identityAbstractor">Serviço para abstração das operações de identidade (login, validação, etc.).</param>
        public LoginCommandHandler(IIdentityAbstractor identityAbstractor)
        {
            _identityAbstractor = identityAbstractor;
        }

        /// <summary>
        /// Manipula a requisição de login, valida o usuário e senha, e retorna os tokens de autenticação.
        /// </summary>
        /// <param name="request">O comando de login contendo o nome de usuário e senha.</param>
        /// <param name="cancellationToken">Token de cancelamento para controlar o processo assíncrono.</param>
        /// <returns>Resposta do login contendo o token de acesso, data de expiração, e funções do usuário.</returns>
        /// <exception cref="UnauthorizedAccessException">Lançado se o usuário não for encontrado ou as credenciais estiverem incorretas.</exception>
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
                ExpirationDate = DateTime.UtcNow.AddHours(1), // Exemplo de expiração
                Roles = roles
            };
        }

        /// <summary>
        /// Gera um token de acesso para o usuário.
        /// Este método pode ser alterado para usar uma abordagem de geração de token mais robusta, como JWT.
        /// </summary>
        /// <param name="user">Usuário autenticado para quem o token será gerado.</param>
        /// <returns>Token de acesso gerado.</returns>
        private string GenerateAccessToken(Domain.Entities.User user)
        {
            return Guid.NewGuid().ToString(); // Gerador de token simplificado; pode ser substituído por uma implementação mais segura.
        }
    }
}