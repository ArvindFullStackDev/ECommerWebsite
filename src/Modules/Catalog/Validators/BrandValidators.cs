using Catalog.Commands.CreateBrand;
using Catalog.Commands.UpdateBrand;
using FluentValidation;

namespace Catalog.Validators;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(200).WithMessage("Brand name must not exceed 200 characters");
    }
}

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid brand ID");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(200).WithMessage("Brand name must not exceed 200 characters");
    }
}
