using FluentAssertions;
using Moq;
using RO.DevTest.Application.Contracts.Infrastructure;
using RO.DevTest.Application.Features.User.Commands.CreateUserCommand;
using RO.DevTest.Domain.Enums;
using RO.DevTest.Domain.Exception;
using Xunit;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

// Alias para evitar conflito com o namespace "User"
using UserEntity = RO.DevTest.Domain.Entities.User;

namespace RO.DevTest.Tests.Unit.Application.Features.User.Commands;

public class CreateUserCommandHandlerTests {
    private readonly Mock<IIdentityAbstractor> _identityAbstractorMock = new();
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests() {
        _sut = new(_identityAbstractorMock.Object);
    }

    [Fact(DisplayName = "Given invalid email should throw a BadRequestException")]
    public void Handle_WhenEmailIsNullOrEmpty_ShouldRaiseABadRequestExcpetion() {
        // Arrange
        string email = string.Empty, password = Guid.NewGuid().ToString();
        CreateUserCommand command = new() {
            Email = email,
            UserName = "user_test",
            Password = password,
            PasswordConfirmation = password,
            Name = "Test User"
        };

        // Act
        Func<Task> action = async () => await _sut.Handle(command, new CancellationToken());

        // Assert
        action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact(DisplayName = "Given passwords not matching should throw a BadRequestException")]
    public void Handle_WhenPasswordDoesntMatchPasswordConfirmation_ShouldRaiseABadRequestException() {
        // Arrange
        string email = "mytestemail@someprovider.com"
            , password = Guid.NewGuid().ToString()
            , passwordConfirmation = Guid.NewGuid().ToString();
        CreateUserCommand command = new() {
            Email = email,
            UserName = "user_test",
            Password = password,
            PasswordConfirmation = passwordConfirmation,
            Name = "Test User"
        };

        // Act
        Func<Task> action = async () => await _sut.Handle(command, new CancellationToken());

        // Assert
        action.Should().ThrowAsync<BadRequestException>();
    }

    [Fact(DisplayName = "Given valid data should create a user successfully")]
    public async Task Handle_WhenDataIsValid_ShouldCreateUserSuccessfully() {
        // Arrange
        string password = "MySecurePass123";
        CreateUserCommand command = new() {
            Email = "valid@email.com",
            UserName = "valid_user",
            Password = password,
            PasswordConfirmation = password,
            Name = "Valid User",
            Role = UserRoles.Customer
        };

        var createdUser = new UserEntity {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            Name = command.Name,
            UserName = command.UserName
        };

        _identityAbstractorMock
            .Setup(x => x.CreateUserAsync(It.IsAny<UserEntity>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _identityAbstractorMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), command.Role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.UserName.Should().Be(command.UserName);
        result.Name.Should().Be(command.Name);
    }
}
