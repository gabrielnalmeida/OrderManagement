namespace OrderManagement.Application.Buyers.Commands.CreateBuyer;

public class CreateBuyerCommandValidator : AbstractValidator<CreateBuyerCommand>
{
    public CreateBuyerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");
    }
}
