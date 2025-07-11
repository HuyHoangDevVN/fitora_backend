﻿using AuthService.Application.DTOs.Users.Requests;
using AuthService.Application.DTOs.Users.Responses;

namespace AuthService.Application.Auths.Commands.EditInForUser;

public class EditInForUserHandler
(IUserRepository repository)
: ICommandHandler<EditInForUserCommand, EditInForUserResult>
{
    public async Task<EditInForUserResult> Handle(EditInForUserCommand request, CancellationToken cancellationToken)
    {
        var editInForUserRequest = EditInForCommandToDto(request);
        var result = await repository.EditInForUserAsync(editInForUserRequest);
        return EditUserResponseToUserResult(result);
    }

    private static EditInfoUserRequest EditInForCommandToDto(EditInForUserCommand command)
        => new EditInfoUserRequest(command.UserId, command.FullName, command.PhoneNumber, command.Avatar);

    private static EditInForUserResult EditUserResponseToUserResult(GetUserResponseDto dto)
        => new EditInForUserResult(dto.UserId, dto.Email, dto.Username, dto.FullName, dto.PhoneNumber, dto.Avatar);
}