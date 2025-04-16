using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetGroupById;

public record GetGroupByIdQuery(Guid Id) : IQuery<GroupDto>;