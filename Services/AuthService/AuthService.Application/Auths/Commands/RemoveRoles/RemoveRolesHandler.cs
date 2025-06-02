using AuthService.Application.Auths.Commands.RemoveRoles;
using AuthService.Application.DTOs.Roles.Requests;

namespace AuthService.Application.Auths.Commands.RemoveRoles;

public class RemoveRolesHandler
(IRoleRepository roleRepository)
: ICommandHandler<RemoveRolesCommand, RemoveRolesResult>
{
    public async Task<RemoveRolesResult> Handle(RemoveRolesCommand command, CancellationToken cancellationToken)
    {
        var request = AssignRoleCommandToDto(command);
        var result = await roleRepository.RemoveRolesAsync(request, cancellationToken);
        return new RemoveRolesResult(result);
    }

    private static AssignRoleRequestDto AssignRoleCommandToDto(RemoveRolesCommand command)
        => new AssignRoleRequestDto(command.RoleNames, command.Email);

}