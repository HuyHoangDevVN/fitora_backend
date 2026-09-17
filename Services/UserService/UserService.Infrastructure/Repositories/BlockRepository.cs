using System.Linq.Expressions;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.Services.IServices;

namespace UserService.Infrastructure.Repositories;

public class BlockRepository : IBlockRepository
{
    private readonly IRepositoryBase<Block> _blockRepo;

    public BlockRepository(IRepositoryBase<Block> blockRepo)
    {
        _blockRepo = blockRepo;
    }

    public async Task<ResponseDto> BlockUserAsync(Guid blockerId, Guid blockedUserId)
    {
        if (blockerId == Guid.Empty || blockedUserId == Guid.Empty)
            return new ResponseDto(null, false, "UserId không hợp lệ.");
        if (blockerId == blockedUserId)
            return new ResponseDto(null, false, "Không thể chặn chính mình.");

        var exists = await _blockRepo.GetAsync(b => b.BlockerUserId == blockerId && b.BlockedUserId == blockedUserId);
        if (exists != null)
            return new ResponseDto(null, false, "Đã chặn người dùng này rồi.");

        var block = new Block
        {
            Id = Guid.NewGuid(),
            BlockerUserId = blockerId,
            BlockedUserId = blockedUserId
        };
        await _blockRepo.AddAsync(block);
        var ok = await _blockRepo.SaveChangesAsync() > 0;
        return new ResponseDto(null, ok, ok ? "Đã chặn người dùng." : "Chặn thất bại.");
    }

    public async Task<ResponseDto> UnblockUserAsync(Guid blockerId, Guid blockedUserId)
    {
        if (blockerId == Guid.Empty || blockedUserId == Guid.Empty)
            return new ResponseDto(null, false, "UserId không hợp lệ.");

        var exists = await _blockRepo.GetAsync(b => b.BlockerUserId == blockerId && b.BlockedUserId == blockedUserId);
        if (exists == null)
            return new ResponseDto(null, false, "Bạn chưa chặn người dùng này.");

        await _blockRepo.DeleteAsync(b => b.BlockerUserId == blockerId && b.BlockedUserId == blockedUserId);
        var ok = await _blockRepo.SaveChangesAsync() > 0;
        return new ResponseDto(null, ok, ok ? "Đã bỏ chặn." : "Bỏ chặn thất bại.");
    }

    public async Task<PaginatedResult<Block>> GetBlockedUsersAsync(Guid blockerId, int pageIndex, int pageSize)
    {
        // Dùng GetPageWithIncludes để trả kèm BlockedUser + UserInfo cho FE
        var includes = new List<Expression<Func<Block, object>>>
        {
            b => b.BlockedUser!,
            b => b.BlockedUser!.UserInfo
        };
        return await _blockRepo.GetPageWithIncludesAsync(
            paginationRequest: new PaginationRequest(pageIndex, pageSize),
            selector: b => b,
            conditions: b => b.BlockerUserId == blockerId,
            includes: includes,
            cancellationToken: CancellationToken.None
        );
    }
}
