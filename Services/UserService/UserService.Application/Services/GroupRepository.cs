using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Services;

public class GroupRepository : IGroupRepository
{
    private readonly IRepositoryBase<Group> _groupRepo;
    private readonly IMapper _mapper;

    public GroupRepository(IRepositoryBase<Group> groupRepo, IMapper mapper)
    {
        _groupRepo = groupRepo;
        _mapper = mapper;
    }

    public async Task<bool> CreateAsync(Group group)
    {
        await _groupRepo.AddAsync(group);
        return await _groupRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Group group)
    {
        await _groupRepo.UpdateAsync(g => g.Id == group.Id, group);
        return await _groupRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _groupRepo.DeleteAsync(g => g.Id == id);
        return await _groupRepo.SaveChangesAsync() > 0;
    }
}