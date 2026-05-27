using FluentValidation;

namespace Atlas.Application.Companies.GetCompanyBySiren;

internal sealed class GetCompanyBySirenValidator : AbstractValidator<GetCompanyBySirenQuery>
{
    public GetCompanyBySirenValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Siren).NotEmpty();
    }
}
