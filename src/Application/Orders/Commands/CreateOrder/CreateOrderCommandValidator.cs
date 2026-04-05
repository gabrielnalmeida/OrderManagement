using OrderManagement.Application.OrderItems.Models;

namespace OrderManagement.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(v => v.BuyerId)
            .NotEmpty().WithMessage("ID do comprador não pode ser vazio.");
            
        RuleFor(v => v.Items)
            .NotEmpty()
            .WithMessage("A ordem deve conter pelo menos um item.");

        RuleForEach(v => v.Items)
            .SetValidator(new SlimOrderItemDtoValidator());
        
    }
}
