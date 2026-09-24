using FluentValidation;

namespace UserService.Application.Usecases.Group.Commands.UpdateGroup;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Request.Id)
            .NotEmpty().WithMessage("Id nhóm không hợp lệ");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên nhóm không được để trống")
            .MaximumLength(100).WithMessage("Tên nhóm không được vượt quá 100 ký tự");

        RuleFor(x => x.Request.Description)
            .MaximumLength(2000).WithMessage("Mô tả nhóm không được vượt quá 2000 ký tự");
    }
}
