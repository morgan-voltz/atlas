using Atlas.Domain.Users;
using Atlas.Domain.Users.Events;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Atlas.Infrastructure.Persistence.IntegrationTests;

/// <summary>
/// Valide la sémantique « after-commit » du dispatch d'événements domaine
/// (cf. <see cref="AtlasDbContext.SaveChangesAsync"/>).
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class DomainEventDispatchTests(PostgresFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChangesAsync_publishes_domain_events_after_commit()
    {
        IPublisher publisher = Substitute.For<IPublisher>();
        await using AtlasDbContext context = CreateContextWithPublisher(publisher);

        EmailAddress email = EmailAddress.Create($"dispatch-{Guid.NewGuid():N}@atlas.test").Value!;
        User user = User.Register(
            UserId.New(),
            email,
            PasswordHash.FromHash("argon2-hash"),
            "tokenhash",
            Now,
            TimeSpan.FromHours(24));

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        await publisher.Received(1).Publish(
            Arg.Is<UserRegisteredDomainEvent>(e => e.Email == email),
            Arg.Any<CancellationToken>());

        user.DomainEvents.Should().BeEmpty(
            "ClearDomainEvents doit être appelé après la collecte pour éviter les double-publications.");
    }

    [Fact]
    public async Task SaveChangesAsync_does_not_publish_when_no_events()
    {
        IPublisher publisher = Substitute.For<IPublisher>();
        await using AtlasDbContext context = CreateContextWithPublisher(publisher);

        await context.SaveChangesAsync();

        await publisher.DidNotReceive().Publish(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    private AtlasDbContext CreateContextWithPublisher(IPublisher publisher)
    {
        DbContextOptions<AtlasDbContext> options = new DbContextOptionsBuilder<AtlasDbContext>()
            .UseNpgsql(fixture.GetConnectionString())
            .Options;
        return new AtlasDbContext(options, publisher);
    }
}
