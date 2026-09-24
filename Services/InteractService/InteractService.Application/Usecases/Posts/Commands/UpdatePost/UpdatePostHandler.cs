namespace InteractService.Application.Usecases.Posts.Commands.UpdatePost;

public class UpdatePostHandler : ICommandHandler<UpdatePostCommand, PostResponseDto>
{
    private readonly IPostRepository _postRepo;
    private readonly IMapper _mapper;

    // 15 phút sau khi đăng mà bài đã Approved thì author không còn được sửa (13.3 / 26.3).
    // FE countdown chỉ là hiển thị; backend là chân lý. Pending thì tác giả vẫn sửa được.
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    public UpdatePostHandler(IPostRepository postRepo, IMapper mapper)
    {
        _postRepo = postRepo;
        _mapper = mapper;
    }

    public async Task<PostResponseDto> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var existingPost = await _postRepo.GetByIdAsync(command.Id);
        if (existingPost is null) throw new NotFoundException("Post not found");

        // 13.1/26.21: phải là tác giả mới được sửa; xóa/uỷ quyền khác handle ở Delete.
        if (existingPost.UserId != command.CurrentUserId)
            throw new UnAuthorizationException("Bạn không có quyền chỉnh sửa bài viết này");

        // Already-approved group posts (hay bất kỳ bài có IsApproved==true) — hết 15 phút thì chặn.
        // Post thường IsApproved==null hoặc false (không bật duyệt) thì skip rule này.
        if (existingPost.IsApproved == true
            && existingPost.CreatedAt.HasValue
            && DateTime.UtcNow - existingPost.CreatedAt.Value > EditWindow)
        {
            throw new BadRequestException("Hết thời gian chỉnh sửa: bài đã đăng quá 15 phút kể từ lúc tạo. Vui lòng tạo bài mới hoặc liên hệ moderator.");
        }

        _mapper.Map(command.Request, existingPost);

        var isUpdated = await _postRepo.UpdateAsync(existingPost);
        if (!isUpdated) throw new InvalidOperationException("Failed to update post");

        return _mapper.Map<PostResponseDto>(existingPost);
    }
}
