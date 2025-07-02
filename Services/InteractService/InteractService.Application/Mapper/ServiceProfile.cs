using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Category.Response;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Application.DTOs.Report.Responses;

namespace InteractService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        #region Post

        CreateMap<CreatePostRequest, Post>().ReverseMap();
        CreateMap<UpdatePostRequest, Post>().ReverseMap();
        CreateMap<Post, PostResponseDto>().ReverseMap();

        #endregion

        #region Category

        CreateMap<CreateCategoryRequest, Category>().ReverseMap();
        CreateMap<CategoryResponseDto, Category>().ReverseMap();
        CreateMap<UpdateCategoryRequest, Category>().ReverseMap();

        #endregion

        #region Comment

        CreateMap<CreateCommentRequest, Comment>().ReverseMap();
        CreateMap<Comment, CommentResponseDto>().ReverseMap();
        CreateMap<UpdateCommentRequest, Comment>().ReverseMap();

        #endregion

        #region Report

        CreateMap<CreateReportRequest, Report>().ReverseMap();
        CreateMap<Report, ReportResponseDto>().ReverseMap();
        CreateMap<PaginatedResult<ReportResponseDto>, PaginatedResult<Report>>().ReverseMap();

        #endregion
    }
}