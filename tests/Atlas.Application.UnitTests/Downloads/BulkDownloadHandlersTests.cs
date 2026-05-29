using Atlas.Application.Downloads.DownloadBulkArchive;
using Atlas.Application.Downloads.GetBulkDownload;
using Atlas.Application.Downloads.RequestBulkDownload;
using Atlas.Domain.Common;
using Atlas.Domain.Downloads;
using Atlas.Domain.Storage;
using Atlas.Domain.Users;
using Atlas.Shared.Result;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Application.UnitTests.Downloads;

public class BulkDownloadHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 28, 10, 0, 0, TimeSpan.Zero);

    // SIREN valides (passent le Luhn) : Carrefour, Air France, EDF.
    private const string Siren1 = "652014051";
    private const string Siren2 = "420495178";
    private const string Siren3 = "552081317";

    private readonly IBulkDownloadJobRepository _jobs = Substitute.For<IBulkDownloadJobRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public BulkDownloadHandlersTests()
    {
        _clock.UtcNow.Returns(Now);
    }

    // ── Request ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Request_returns_empty_sirens_when_list_is_empty()
    {
        var handler = new RequestBulkDownloadHandler(_jobs, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RequestBulkDownloadCommand(Guid.NewGuid(), []),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.empty_sirens");
    }

    [Fact]
    public async Task Request_returns_too_many_when_above_limit()
    {
        // 51 SIREN identiques : on dépasse MaxSirens (50). On vise downloads.too_many_sirens
        // sans déclencher le invalid_siren auparavant.
        IReadOnlyList<string> tooMany = Enumerable.Repeat(Siren1, BulkDownloadJob.MaxSirens + 1).ToArray();
        var handler = new RequestBulkDownloadHandler(_jobs, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RequestBulkDownloadCommand(Guid.NewGuid(), tooMany),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.too_many_sirens");
    }

    [Fact]
    public async Task Request_returns_invalid_siren_when_one_is_malformed()
    {
        var handler = new RequestBulkDownloadHandler(_jobs, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RequestBulkDownloadCommand(Guid.NewGuid(), [Siren1, "ABCD"]),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.invalid_siren");
        await _jobs.DidNotReceive().AddAsync(Arg.Any<BulkDownloadJob>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Request_creates_and_persists_job_on_success()
    {
        Guid userId = Guid.NewGuid();
        var handler = new RequestBulkDownloadHandler(_jobs, _clock, _unitOfWork);

        Result<Guid> result = await handler.Handle(
            new RequestBulkDownloadCommand(userId, [Siren1, Siren2, Siren3]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _jobs.Received(1).AddAsync(
            Arg.Is<BulkDownloadJob>(j =>
                j.UserId == new UserId(userId) &&
                j.Status == BulkDownloadStatus.Pending &&
                j.SirenList.Count == 3),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── Get ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_returns_not_found_when_job_missing()
    {
        var handler = new GetBulkDownloadHandler(_jobs);
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>())
            .Returns((BulkDownloadJob?)null);

        Result<BulkDownloadJobDto> result = await handler.Handle(
            new GetBulkDownloadQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.not_found");
    }

    [Fact]
    public async Task Get_returns_not_found_when_owned_by_other_user()
    {
        Guid otherUser = Guid.NewGuid();
        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(otherUser), [Siren1], Now, TimeSpan.FromHours(24));
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>()).Returns(job);

        var handler = new GetBulkDownloadHandler(_jobs);

        Result<BulkDownloadJobDto> result = await handler.Handle(
            new GetBulkDownloadQuery(Guid.NewGuid(), job.Id.Value),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.not_found");
    }

    [Fact]
    public async Task Get_returns_dto_when_owned_by_user()
    {
        Guid userId = Guid.NewGuid();
        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(userId), [Siren1, Siren2], Now, TimeSpan.FromHours(24));
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>()).Returns(job);

        var handler = new GetBulkDownloadHandler(_jobs);

        Result<BulkDownloadJobDto> result = await handler.Handle(
            new GetBulkDownloadQuery(userId, job.Id.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sirens.Should().BeEquivalentTo(new[] { Siren1, Siren2 });
        result.Value.Status.Should().Be("Pending");
    }

    // ── Download archive ────────────────────────────────────────────────────────

    [Fact]
    public async Task Download_returns_not_ready_when_status_is_pending()
    {
        Guid userId = Guid.NewGuid();
        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(userId), [Siren1], Now, TimeSpan.FromHours(24));
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>()).Returns(job);

        var handler = new DownloadBulkArchiveHandler(_jobs, _storage, _clock);

        Result<BulkArchiveContent> result = await handler.Handle(
            new DownloadBulkArchiveQuery(userId, job.Id.Value),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.not_ready");
    }

    [Fact]
    public async Task Download_returns_expired_when_past_ttl()
    {
        Guid userId = Guid.NewGuid();
        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(userId), [Siren1], Now, TimeSpan.FromHours(24));
        job.MarkAsReady("bulk/abc.zip", Now);
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        _clock.UtcNow.Returns(Now.AddHours(48));

        var handler = new DownloadBulkArchiveHandler(_jobs, _storage, _clock);

        Result<BulkArchiveContent> result = await handler.Handle(
            new DownloadBulkArchiveQuery(userId, job.Id.Value),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("downloads.expired");
    }

    [Fact]
    public async Task Download_returns_stream_when_ready_and_not_expired()
    {
        Guid userId = Guid.NewGuid();
        BulkDownloadJob job = BulkDownloadJob.Request(
            new UserId(userId), [Siren1], Now, TimeSpan.FromHours(24));
        job.MarkAsReady("bulk/abc.zip", Now);
        _jobs.GetByIdAsync(Arg.Any<BulkDownloadJobId>(), Arg.Any<CancellationToken>()).Returns(job);
        var fakeStream = new MemoryStream([0x50, 0x4B]); // "PK" : signature ZIP minimaliste
        _storage.OpenReadAsync("bulk/abc.zip", Arg.Any<CancellationToken>()).Returns(fakeStream);

        var handler = new DownloadBulkArchiveHandler(_jobs, _storage, _clock);

        Result<BulkArchiveContent> result = await handler.Handle(
            new DownloadBulkArchiveQuery(userId, job.Id.Value),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FileName.Should().StartWith("atlas-bulk-").And.EndWith(".zip");
        result.Value.Stream.Should().BeSameAs(fakeStream);
    }
}
