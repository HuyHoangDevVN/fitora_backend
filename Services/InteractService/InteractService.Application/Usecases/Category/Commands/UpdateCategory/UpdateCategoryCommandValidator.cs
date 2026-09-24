using FluentValidation;

namespace InteractService.Application.Usecases.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");
        RuleFor(x => x.Request.Description)
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự")
            .When(x => x.Request.Description is not null);
    }
}
