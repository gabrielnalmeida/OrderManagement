using OrderManagement.Application.OrderItems.Models;

namespace OrderManagement.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("O Id da ordem deve ser maior que zero.");
            
        RuleFor(v => v.Items)
            .NotEmpty()
            .WithMessage("A ordem deve conter pelo menos um item.");

        RuleForEach(v => v.Items)
            .SetValidator(new SlimOrderItemDtoValidator());
    }
}
