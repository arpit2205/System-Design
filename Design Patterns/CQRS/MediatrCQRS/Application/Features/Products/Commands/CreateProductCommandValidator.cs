using FluentValidation;

namespace Application.Features.Products.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.createProductDto.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100);

        RuleFor(x => x.createProductDto.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");
    }
}