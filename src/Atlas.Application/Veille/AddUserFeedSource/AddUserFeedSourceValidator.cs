using Atlas.Domain.Veille;
using FluentValidation;

namespace Atlas.Application.Veille.AddUserFeedSource;

internal sealed class AddUserFeedSourceValidator : AbstractValidator<AddUserFeedSourceCommand>
{
    public AddUserFeedSourceValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Url)
            .NotEmpty()
            .MaximumLength(FeedSource.MaxUrlLength)
            .Must(BeAbsoluteHttpUrl).WithMessage("URL http(s) absolue requise.");

        RuleFor(command => command.Name)
            .MaximumLength(FeedSource.MaxNameLength)
            .When(command => !string.IsNullOrWhiteSpace(command.Name));
    }

    private static bool BeAbsoluteHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
