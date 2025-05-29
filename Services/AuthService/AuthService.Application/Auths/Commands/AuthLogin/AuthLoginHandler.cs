using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.Services.IServices;
using AutoMapper;
using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.AuthLogin;

public class AuthLoginHandler(IAuthRepository repository, IMapper mapper)
    : ICommandHandler<AuthLoginCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(AuthLoginCommand command, CancellationToken cancellationToken)
    {
        var request = mapper.Map<LoginRequestDto>(command);
        var response = await repository.LoginAsync(request);
        return response;
    }
}