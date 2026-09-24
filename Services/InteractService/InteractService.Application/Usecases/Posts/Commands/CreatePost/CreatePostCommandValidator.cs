using FluentValidation;

namespace InteractService.Application.Usecases.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Request.UserId).NotEmpty();
        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Nội dung bài viết không được để trống")
            .MaximumLength(5000).WithMessage("Nội dung bài viết không được vượt quá 5000 ký tự")
            .When(x => string.IsNullOrWhiteSpace(x.Request.MediaUrl));
        RuleFor(x => x.Request.Content)
            .MaximumLength(5000).WithMessage("Nội dung bài viết không được vượt quá 5000 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.MediaUrl));
    }
}
