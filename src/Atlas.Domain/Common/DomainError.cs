using Atlas.Shared.Result;

namespace Atlas.Domain.Common;

public abstract record DomainError(string Code, string Message) : Error(Code, Message);
