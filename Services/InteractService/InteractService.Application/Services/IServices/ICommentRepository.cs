using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;

namespace InteractService.Application.Services.IServices;

public interface ICommentRepository
{
    Task<bool> CreateAsync(Comment comment);
    Task<Comment> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Comment comment);
    Task<bool> VoteAsync(VoteCommentRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<PaginatedCursorResult<CommentResponseDto>> GetByPost(GetPostCommentsRequest request);
    Task<PaginatedCursorResult<CommentResponseDto>> GetReplies(GetCommentRepliesRequest request);
}