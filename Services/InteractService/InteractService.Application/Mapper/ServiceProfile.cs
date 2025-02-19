using AutoMapper;

namespace InteractService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<CreatePostRequest, Post>();
        CreateMap<Post, PostResponseDto>();
        CreateMap<UpdatePostRequest, Post>();
        CreateMap<Post, PostResponseDto>();
    }
}