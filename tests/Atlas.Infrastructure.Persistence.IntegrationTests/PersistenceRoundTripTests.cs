using Atlas.Domain.Inpi;
using Atlas.Domain.Users;
using Atlas.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Round-trips contre un PostgreSQL réel : valident les conversions de value objects, le mapping des
/// champs privés, les index uniques et l'application des migrations.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PersistenceRoundTripTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task User_round_trips_with_value_object_conversions()
    {
        User user = NewActiveUser(UniqueEmail());

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            User loaded = await context.Users.SingleAsync(candidate => candidate.Id == user.Id);
            loaded.Email.Value.Should().Be(user.Email.Value);
            loaded.PasswordHash.Value.Should().Be("argon2-hash");
            loaded.Status.Should().Be(UserStatus.Active);
            loaded.EmailVerifiedAt.Should().Be(Now);
        }
    }

    [Fact]
    public async Task Duplicate_email_violates_unique_index()
    {
        string email = UniqueEmail();

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(NewActiveUser(email));
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.Users.AddAsync(NewActiveUser(email));
            Func<Task> act = async () => await context.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }

    [Fact]
    public async Task InpiCredentials_round_trip_keeps_encrypted_values()
    {
        var credentials = InpiCredentials.Create(UserId.New(), "enc-user", "enc-pass", Now);

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.InpiCredentials.AddAsync(credentials);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            InpiCredentials loaded = await context.InpiCredentials.SingleAsync(c => c.Id == credentials.Id);
            loaded.EncryptedUsername.Should().Be("enc-user");
            loaded.EncryptedPassword.Should().Be("enc-pass");
            loaded.Status.Should().Be(InpiCredentialsStatus.Active);
            loaded.UserId.Should().Be(credentials.UserId);
        }
    }

    [Fact]
    public async Task RefreshToken_round_trips_and_is_queryable_by_hash()
    {
        string hash = "hash-" + Guid.NewGuid().ToString("N");
        var token = RefreshToken.Issue(RefreshTokenId.New(), UserId.New(), hash, Now, TimeSpan.FromDays(30));

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();
        }

        await using (AtlasDbContext context = fixture.CreateContext())
        {
            RefreshToken loaded = await context.RefreshTokens.SingleAsync(t => t.TokenHash == hash);
            loaded.IsActive(Now.AddDays(1)).Should().BeTrue();
            loaded.ExpiresAt.Should().BeCloseTo(Now.AddDays(30), TimeSpan.FromSeconds(1));
        }
    }

    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    private static User NewActiveUser(string email)
    {
        EmailAddress address = EmailAddress.Create(email).Value!;
        var user = User.Register(UserId.New(), address, PasswordHash.FromHash("argon2-hash"), "tokenhash", Now, TimeSpan.FromHours(24));
        user.ConfirmEmail("tokenhash", Now);
        return user;
    }
}
