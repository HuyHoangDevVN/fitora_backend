namespace UserService.Application.DTOs.GroupPost.Requests;

public record CreateGroupPostRequest(
    Guid PostId,
    Guid GroupId,
    Guid AuthorId,
    bool IsApproved
);