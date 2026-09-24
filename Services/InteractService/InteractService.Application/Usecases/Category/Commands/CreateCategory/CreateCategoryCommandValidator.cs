using FluentValidation;

namespace InteractService.Application.Usecases.Category.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");
        RuleFor(x => x.Request.Description)
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự")
            .When(x => x.Request.Description is not null);
        RuleFor(x => x.Request.Slug)
            .MaximumLength(150)
            .When(x => x.Request.Slug is not null);
    }
}
