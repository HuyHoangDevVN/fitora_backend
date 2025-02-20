namespace InteractService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<CreatePostRequest, Post>().ReverseMap();
        CreateMap<UpdatePostRequest, Post>().ReverseMap();
        CreateMap<Post, PostResponseDto>().ReverseMap();
    }
}