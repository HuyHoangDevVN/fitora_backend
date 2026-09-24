using FluentValidation;

namespace InteractService.Application.Usecases.Comments.Commands.UpdateComment;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống")
            .MaximumLength(2000).WithMessage("Nội dung bình luận không được vượt quá 2000 ký tự")
            .When(x => string.IsNullOrWhiteSpace(x.Request.MediaUrl));
    }
}
