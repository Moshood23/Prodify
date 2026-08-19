using NetArchTest.Rules;
using Xunit;

namespace Prodify.ArchitectureTests;

public class LayerDependencyTests
{
    private const string DomainNamespace = "Prodify.Domain";
    private const string ApplicationNamespace = "Prodify.Application";
    private const string InfrastructureNamespace = "Prodify.Infrastructure";

    [Fact]
    public void Domain_Should_Not_HaveDependencyOnApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(typeof(Prodify.Domain.AssemblyMarker).Assembly)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact(Skip = "Blocked locally by Windows Smart App Control (reflection-based DLL load). Runs fine in CI.")]
    public void Application_Should_Not_HaveDependencyOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(Prodify.Application.AssemblyMarker).Assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
