namespace UserService.Application.Usecases.GroupEvent.Queries.GetGroupEventById;

public record GetGroupEventByIdQuery(Guid EventId) : IQuery<Domain.Models.GroupEvent>;
