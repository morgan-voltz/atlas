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
    private const string ApplicationPremiumNamespace = "Atlas.Application.Premium";
    private const string InfrastructureNamespace = "Atlas.Infrastructure";
    private const string ApiNamespace = "Atlas.Api";

    private static readonly Assembly DomainAssembly = typeof(User).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Atlas.Application.DependencyInjection).Assembly;
    private static readonly Assembly ApplicationPremiumAssembly = typeof(Atlas.Application.Premium.AssemblyMarker).Assembly;
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

    /// <summary>
    /// F-050 — Le cœur open source <c>Atlas.Application</c> ne doit JAMAIS référencer
    /// <c>Atlas.Application.Premium</c>. La séparation est la condition de la stratégie
    /// open core (cf. ADR-006) : la frontière premium se ferme côté hébergé via les ports
    /// déclarés dans <c>Atlas.Domain.Veille.Premium</c>, sans contamination du cœur.
    /// </summary>
    [Fact]
    public void Application_should_not_depend_on_application_premium()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationPremiumNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    /// <summary>
    /// F-050 — Le domaine ne dépend de rien (sauf <c>Atlas.Shared</c>) ; <c>Atlas.Application.Premium</c>
    /// y compris. Doublon défensif du test précédent côté domaine.
    /// </summary>
    [Fact]
    public void Domain_should_not_depend_on_application_premium()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationPremiumNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    /// <summary>
    /// F-050 — <c>Atlas.Application.Premium</c> est une couche de use cases : elle peut
    /// orchestrer le domaine et le cœur Application, mais ne doit jamais référencer un
    /// adapter d'infrastructure ni l'API. Les implémentations des ports premium vivront
    /// dans des projets <c>Atlas.Infrastructure.*Premium</c> dédiés, à câbler en DI dans
    /// la composition root (Atlas.Api).
    /// </summary>
    [Fact]
    public void Application_premium_should_not_depend_on_infrastructure_or_api()
    {
        TestResult result = Types.InAssembly(ApplicationPremiumAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Types en violation : " + string.Join(", ", result.FailingTypeNames ?? []);
}
