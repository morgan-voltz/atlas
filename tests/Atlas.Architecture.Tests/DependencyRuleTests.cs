using System.Reflection;
using Atlas.Domain.Users;
using FluentAssertions;
using NetArchTest.Rules;

namespace Atlas.Architecture.Tests;

/// <summary>
/// Vérifie les « règles d'or » de dépendance de l'architecture hexagonale (cf. CLAUDE.md et docs/09).
/// </summary>
public class DependencyRuleTests
{
    private const string ApplicationNamespace = "Atlas.Application";
    private const string InfrastructureNamespace = "Atlas.Infrastructure";
    private const string ApiNamespace = "Atlas.Api";

    private static readonly Assembly DomainAssembly = typeof(User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Atlas.Application.DependencyInjection).Assembly;
    private static readonly Assembly PersistenceAssembly = typeof(Atlas.Infrastructure.Persistence.AtlasDbContext).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_application_infrastructure_or_api()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    [Fact]
    public void Domain_should_not_depend_on_external_infrastructure_libraries()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "MediatR", "Npgsql")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    [Fact]
    public void Infrastructure_persistence_should_not_depend_on_api()
    {
        TestResult result = Types.InAssembly(PersistenceAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Types en violation : " + string.Join(", ", result.FailingTypeNames ?? []);
}
