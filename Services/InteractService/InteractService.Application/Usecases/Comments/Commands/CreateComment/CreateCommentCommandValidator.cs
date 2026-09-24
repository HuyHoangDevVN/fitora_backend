using FluentValidation;

namespace InteractService.Application.Usecases.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Request.PostId).NotEmpty();
        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống")
            .MaximumLength(2000).WithMessage("Nội dung bình luận không được vượt quá 2000 ký tự")
            .When(x => string.IsNullOrWhiteSpace(x.Request.MediaUrl));
    }
}
