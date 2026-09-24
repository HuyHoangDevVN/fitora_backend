using FluentValidation;

namespace UserService.Application.Usecases.Group.Commands.CreateGroup;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên nhóm không được để trống")
            .MaximumLength(100).WithMessage("Tên nhóm không được vượt quá 100 ký tự");

        RuleFor(x => x.Request.Description)
            .MaximumLength(2000).WithMessage("Mô tả nhóm không được vượt quá 2000 ký tự");
    }
}
