using FluentValidation;

namespace InteractService.Application.Usecases.Posts.Commands.UpdatePost;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.Request.Content)
            .MaximumLength(5000).WithMessage("Nội dung bài viết không được vượt quá 5000 ký tự")
            .When(x => x.Request.Content is not null);
    }
}
