namespace UserService.Application.DTOs.Follow.Requests;

public record GetFollowersRequest(Guid Id, int PageIndex = 0, int PageSize = 10);