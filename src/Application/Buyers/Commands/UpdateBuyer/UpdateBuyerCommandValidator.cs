namespace OrderManagement.Application.Buyers.Commands.UpdateBuyer;

public class UpdateBuyerCommandValidator : AbstractValidator<UpdateBuyerCommand>
{
    public UpdateBuyerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");
    }
}
