using FluentValidation;
using FinanSmartAI.Application.DTOs;

namespace FinanSmartAI.Application.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo de transação é obrigatório")
            .Must(x => x == "receita" || x == "despesa")
            .WithMessage("Tipo deve ser 'receita' ou 'despesa'");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .Length(1, 255).WithMessage("Descrição deve ter entre 1 e 255 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(9999999.99m).WithMessage("Valor não pode exceder 9.999.999,99");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória")
            .Length(1, 100).WithMessage("Categoria deve ter entre 1 e 100 caracteres");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Data não pode ser no futuro");
    }
}

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo de transação é obrigatório")
            .Must(x => x == "receita" || x == "despesa")
            .WithMessage("Tipo deve ser 'receita' ou 'despesa'");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .Length(1, 255).WithMessage("Descrição deve ter entre 1 e 255 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(9999999.99m).WithMessage("Valor não pode exceder 9.999.999,99");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória")
            .Length(1, 100).WithMessage("Categoria deve ter entre 1 e 100 caracteres");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Data não pode ser no futuro");
    }
}
