using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.Services.IServices;

namespace UserService.Infrastructure.Repositories;

public class GroupPostRepository : IGroupPostRepository
{
    private readonly IRepositoryBase<GroupPost> _groupPostRepo;

    public GroupPostRepository(IRepositoryBase<GroupPost> groupPostRepo)
    {
        _groupPostRepo = groupPostRepo;
    }

    public async Task<bool> CreateAsync(GroupPost groupPost)
    {
        await _groupPostRepo.AddAsync(groupPost);
        return await _groupPostRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(GroupPost groupPost)
    {
        await _groupPostRepo.UpdateAsync(gp => gp.PostId == groupPost.PostId, groupPost);
        return await _groupPostRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _groupPostRepo.DeleteAsync(gp => gp.PostId == id);
        return await _groupPostRepo.SaveChangesAsync() > 0;
    }
}