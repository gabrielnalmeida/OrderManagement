namespace OrderManagement.Application.OrderItems.Models;

public class SlimOrderItemDtoValidator : AbstractValidator<SlimOrderItemDto>
{
    public SlimOrderItemDtoValidator()
    {
        RuleFor(v => v.ProductId)
            .NotEmpty().WithMessage("ID do produto não pode ser vazio.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser um valor positivo.");
    }
}
