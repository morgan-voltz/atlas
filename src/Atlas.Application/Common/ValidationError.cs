using Atlas.Shared.Result;

namespace Atlas.Application.Common;

public sealed record ValidationFailureDetail(string PropertyName, string Message);

public sealed record ValidationError(IReadOnlyList<ValidationFailureDetail> Failures)
    : Error("validation.failed", "Un ou plusieurs champs sont invalides.");
