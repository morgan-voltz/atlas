using System.Reflection;
using FluentAssertions;
using MediatR;

namespace Atlas.Architecture.Tests;

/// <summary>
/// Conventions de codage vérifiées automatiquement (cf. CLAUDE.md §« Patterns à utiliser
/// systématiquement »). Lot 6 — ferme l'audit profond côté contrôle automatique.
/// </summary>
public class ConventionTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Atlas.Application.DependencyInjection).Assembly;

    private static readonly Assembly ApplicationPremiumAssembly =
        typeof(Atlas.Application.Premium.AssemblyMarker).Assembly;

    /// <summary>
    /// CLAUDE.md : « Les handlers sont <c>internal sealed</c>. » Vérifié pour
    /// <c>Atlas.Application</c> et <c>Atlas.Application.Premium</c>.
    /// Un handler MediatR exposé en <c>public</c> serait une fuite du contrat interne
    /// du module ; un handler non <c>sealed</c> rendrait l'héritage trop facile et briserait
    /// la lisibilité du graphe MediatR.
    /// </summary>
    [Fact]
    public void Application_handlers_must_be_internal_sealed()
    {
        IEnumerable<Type> violations = FindMisShapedHandlers(ApplicationAssembly);

        violations.Should().BeEmpty(
            "Handlers MediatR mal formés : " + string.Join(", ", violations.Select(t => t.FullName)));
    }

    [Fact]
    public void Application_premium_handlers_must_be_internal_sealed()
    {
        IEnumerable<Type> violations = FindMisShapedHandlers(ApplicationPremiumAssembly);

        violations.Should().BeEmpty(
            "Handlers MediatR mal formés : " + string.Join(", ", violations.Select(t => t.FullName)));
    }

    private static IEnumerable<Type> FindMisShapedHandlers(Assembly assembly)
    {
        Type[] handlerInterfaces =
        [
            typeof(IRequestHandler<,>),
            typeof(IRequestHandler<>),
            typeof(INotificationHandler<>),
            typeof(IStreamRequestHandler<,>),
        ];

        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())))
            .Where(t => !t.IsSealed || t.IsPublic);
    }
}
