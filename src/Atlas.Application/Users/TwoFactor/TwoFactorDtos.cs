namespace Atlas.Application.Users.TwoFactor;

public sealed record TwoFactorSetupDto(string Secret, string ProvisioningUri);

public sealed record TwoFactorEnabledDto(IReadOnlyList<string> RecoveryCodes);
