using BuildingBlocks.Security;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Posts.Queries.GetByIdPost;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetPostById;

public class GetPostByIdHandler : IQueryHandler<GetPostByIdQuery, PostResponseDto>
{
    private readonly IPostRepository _postRepo;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _db;
    private readonly IBlockedIdsProvider _blockedIdsProvider;
    private readonly IAuthorizeExtension _auth;

    public GetPostByIdHandler(IPostRepository postRepo, IMapper mapper, IApplicationDbContext db,
        IBlockedIdsProvider blockedIdsProvider, IAuthorizeExtension auth)
    {
        _postRepo = postRepo;
        _mapper = mapper;
        _db = db;
        _blockedIdsProvider = blockedIdsProvider;
        _auth = auth;
    }

    public async Task<PostResponseDto> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await _postRepo.GetByIdAsync(query.Id);
        if (post == null)
            throw new KeyNotFoundException("Post not found");

        var dto = _mapper.Map<PostResponseDto>(post);
        await ShareEnricher.EnrichAsync(_db, new[] { dto }, cancellationToken);

        Guid currentUserId = Guid.Empty;
        bool hasUser = false;
        try
        {
            currentUserId = _auth.GetUserFromClaimToken().Id;
            hasUser = currentUserId != Guid.Empty;
        }
        catch { }

        if (hasUser)
        {
            var filtered = await BlockFilter.ApplyAsync(_blockedIdsProvider, currentUserId, new[] { dto }, cancellationToken);
            if (filtered.Count == 0)
                throw new KeyNotFoundException("Post not found");
            dto = filtered[0];
        }

        if (hasUser)
            await ReactEnricher.EnrichAsync(_db, new[] { dto }, currentUserId, cancellationToken);
        return dto;
    }
}