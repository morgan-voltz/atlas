using FluentValidation;

namespace Atlas.Application.IntellectualProperty.GetTrademark;

internal sealed class GetTrademarkValidator : AbstractValidator<GetTrademarkQuery>
{
    public GetTrademarkValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.DepositNumber).NotEmpty();
    }
}

internal sealed class GetTrademarkImageValidator : AbstractValidator<GetTrademarkImageQuery>
{
    public GetTrademarkImageValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.DepositNumber).NotEmpty();
    }
}
