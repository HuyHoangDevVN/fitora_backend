using AutoMapper;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace InteractService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IMapper _mapper;

    public PostRepository(IRepositoryBase<Post> postRepo, IMapper mapper)
    {
        _postRepo = postRepo;
        _mapper = mapper;
    }
    
    public async Task<bool> CreateAsync(Post post)
    {
        await _postRepo.AddAsync(post);
        var isPostSuccess = await _postRepo.SaveChangesAsync() > 0;
        return isPostSuccess;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _postRepo.GetAllAsync();
    }

    public Task<IEnumerable<Post>> GetListAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        return await _postRepo.GetAsync(x => x.Id == id);
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        await _postRepo.UpdateAsync(p => p.Id == post.Id, post);
        return await _postRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await _postRepo.DeleteAsync(p => p.Id == id); 
            var isDeleted = await _postRepo.SaveChangesAsync() > 0;
            return isDeleted;
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }
}