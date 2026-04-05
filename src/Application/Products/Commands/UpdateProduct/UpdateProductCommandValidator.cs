namespace OrderManagement.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Descrição não pode exceder 500 caracteres.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");
    }
}
