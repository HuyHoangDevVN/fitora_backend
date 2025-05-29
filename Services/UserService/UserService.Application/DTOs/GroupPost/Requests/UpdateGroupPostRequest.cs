namespace UserService.Application.DTOs.GroupPost.Requests;

public record UpdateGroupPostRequest(
    Guid PostId,
    Guid GroupId,
    Guid AuthorId,
    bool IsApproved
);