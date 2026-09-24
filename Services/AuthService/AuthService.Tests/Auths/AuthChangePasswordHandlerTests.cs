using AuthService.Application.Auths.Commands.AuthChangePassword;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.Services.IServices;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthService.Tests.Auths;

public class AuthChangePasswordHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepository = new();
    private readonly AuthChangePasswordHandler _handler;

    public AuthChangePasswordHandlerTests()
    {
        _handler = new AuthChangePasswordHandler(_authRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenRepositorySucceeds_ReturnsSuccessResult()
    {
        _authRepository
            .Setup(r => r.ChangePasswordAsync(It.IsAny<ChangePasswordRequestDto>()))
            .ReturnsAsync(true);

        var command = new AuthChangePasswordCommand("OldPass123!", "NewPass123!", "NewPass123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ReturnsFailureResult()
    {
        _authRepository
            .Setup(r => r.ChangePasswordAsync(It.IsAny<ChangePasswordRequestDto>()))
            .ReturnsAsync(false);

        var command = new AuthChangePasswordCommand("WrongOldPass1!", "NewPass123!", "NewPass123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PassesExactCredentialsToRepository()
    {
        ChangePasswordRequestDto? capturedDto = null;
        _authRepository
            .Setup(r => r.ChangePasswordAsync(It.IsAny<ChangePasswordRequestDto>()))
            .Callback<ChangePasswordRequestDto>(dto => capturedDto = dto)
            .ReturnsAsync(true);

        var command = new AuthChangePasswordCommand("Old1!", "New1!", "New1!");
        await _handler.Handle(command, CancellationToken.None);

        capturedDto.Should().NotBeNull();
        capturedDto!.OldPassword.Should().Be("Old1!");
        capturedDto.NewPassword.Should().Be("New1!");
        capturedDto.ConfirmPassword.Should().Be("New1!");
    }
}
