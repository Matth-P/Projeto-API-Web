using System.Net;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace RO.DevTest.Domain.Exception;

/// <summary>
/// Retorna um <see cref="HttpStatusCode.BadRequest"/> para
/// a requisição com mensagens de erro padronizadas
/// </summary>
public class BadRequestException : ApiException {
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public BadRequestException(IdentityResult result) 
        : base(FormatIdentityErrors(result)) { }

    public BadRequestException(string error) 
        : base(error) { }

    public BadRequestException(ValidationResult validationResult) 
        : base(FormatValidationErrors(validationResult)) { }

    /// <summary>
    /// Formata os erros do IdentityResult para uma string semântica
    /// </summary>
    private static string FormatIdentityErrors(IdentityResult result) {
        StringBuilder errorMessage = new StringBuilder();
        foreach (var error in result.Errors) {
            errorMessage.AppendLine($"- {error.Description}");
        }
        return errorMessage.ToString();
    }

    /// <summary>
    /// Formata os erros do ValidationResult para uma string semântica
    /// </summary>
    private static string FormatValidationErrors(ValidationResult validationResult) {
        StringBuilder errorMessage = new StringBuilder();
        foreach (var error in validationResult.Errors) {
            errorMessage.AppendLine($"- {error.ErrorMessage}");
        }
        return errorMessage.ToString();
    }
}