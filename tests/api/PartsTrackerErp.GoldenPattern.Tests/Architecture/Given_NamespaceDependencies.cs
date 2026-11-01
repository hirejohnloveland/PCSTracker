using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using PartsTrackerErp.GoldenPattern.Api.Controllers;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PartsTrackerErp.GoldenPattern.Tests.Architecture;

/// <summary>
/// Enforces the permitted dependencies in docs/project-structure.md. ArchUnitNET reads the compiled
/// binaries, so run these tests in Debug.
/// </summary>
[Trait(Constants.TestCategory, Constants.ArchitectureCategory)]
public class Given_NamespaceDependencies
{
    private const string CoreRoot = "PartsTrackerErp.GoldenPattern.Core";

    private static readonly ArchUnitNET.Domain.Architecture s_architecture = new ArchLoader()
        .LoadAssemblies(typeof(VendorsApplicationService).Assembly, typeof(ApiControllerBase).Assembly)
        .Build();

    private static readonly IObjectProvider<IType> s_domain = CoreLayer("Domain");
    private static readonly IObjectProvider<IType> s_entities = CoreLayer("Domain.Entities");
    private static readonly IObjectProvider<IType> s_applicationServices = CoreLayer("ApplicationServices");
    private static readonly IObjectProvider<IType> s_applicationServiceAbstractions = CoreLayer("ApplicationServices.Abstractions");
    private static readonly IObjectProvider<IType> s_applicationServiceImplementations = CoreLayer("ApplicationServices.Implementations");
    private static readonly IObjectProvider<IType> s_contracts = CoreLayer("Contracts");
    private static readonly IObjectProvider<IType> s_repositories = CoreLayer("Repositories");
    private static readonly IObjectProvider<IType> s_repositoryAbstractions = CoreLayer("Repositories.Abstractions");
    private static readonly IObjectProvider<IType> s_repositoryImplementations = CoreLayer("Repositories.Implementations");
    private static readonly IObjectProvider<IType> s_integrations = CoreLayer("Integrations");
    private static readonly IObjectProvider<IType> s_integrationAbstractions = CoreLayer("Integrations.Abstractions");
    private static readonly IObjectProvider<IType> s_integrationImplementations = CoreLayer("Integrations.Implementations");
    private static readonly IObjectProvider<IType> s_persistence = CoreLayer("Infrastructure.Persistence");
    private static readonly IObjectProvider<IType> s_validation = CoreLayer("Validation");

    // Selected by assembly so no API type is missed. ArchUnitNET does not analyze top-level statements, so Program.cs is not checked.
    private static readonly IObjectProvider<IType> s_apiAssembly =
        Types().That().ResideInAssembly(typeof(ApiControllerBase).Assembly).As("API assembly");

    // RavenDB.Client ships Sparrow, its low-level library.
    private static readonly IObjectProvider<IType> s_ravenDb = Types(true).That()
        .ResideInNamespaceOrBelow("Raven.Client")
        .Or().ResideInNamespaceOrBelow("Sparrow")
        .As("RavenDB");

    private static readonly IObjectProvider<IType> s_aspNetCore =
        Types(true).That().ResideInNamespaceOrBelow("Microsoft.AspNetCore").As("ASP.NET Core");

    [Fact]
    public void Domain_DoesNotUseOtherLayersAspNetCoreOrRavenDb()
    {
        IArchRule rule = Types().That().Are(s_domain)
            .Should().NotDependOnAny(s_applicationServices)
            .AndShould().NotDependOnAny(s_contracts)
            .AndShould().NotDependOnAny(s_repositories)
            .AndShould().NotDependOnAny(s_integrations)
            .AndShould().NotDependOnAny(s_persistence)
            .AndShould().NotDependOnAny(s_aspNetCore)
            .AndShould().NotDependOnAny(s_ravenDb)
            .Because("project-structure.md, Permitted dependencies: Domain");

        rule.Check(s_architecture);
    }

    [Fact]
    public void ApplicationServices_DoNotUseImplementationsPersistenceOrRavenDb()
    {
        IArchRule rule = Types().That().Are(s_applicationServices)
            .Should().NotDependOnAny(s_repositoryImplementations)
            .AndShould().NotDependOnAny(s_integrationImplementations)
            .AndShould().NotDependOnAny(s_persistence)
            .AndShould().NotDependOnAny(s_ravenDb)
            .Because("project-structure.md, Permitted dependencies: ApplicationServices");

        rule.Check(s_architecture);
    }

    [Fact]
    public void RepositoryAbstractions_DoNotUseImplementationsPersistenceOrRavenDb()
    {
        IArchRule rule = Types().That().Are(s_repositoryAbstractions)
            .Should().NotDependOnAny(s_repositoryImplementations)
            .AndShould().NotDependOnAny(s_persistence)
            .AndShould().NotDependOnAny(s_ravenDb)
            .Because("project-structure.md, Permitted dependencies: Repositories/Abstractions");

        rule.Check(s_architecture);
    }

    [Fact]
    public void IntegrationAbstractions_DoNotUseIntegrationImplementations()
    {
        // Integrations/Abstractions has no types yet. Remove the exemption when it gets one.
        IArchRule rule = Types().That().Are(s_integrationAbstractions)
            .Should().NotDependOnAny(s_integrationImplementations)
            .Because("project-structure.md, Permitted dependencies: Integrations/Abstractions")
            .WithoutRequiringPositiveResults();

        rule.Check(s_architecture);
    }

    [Fact]
    public void Validation_DoesNotUseImplementationsPersistenceOrRavenDb()
    {
        IArchRule rule = Types().That().Are(s_validation)
            .Should().NotDependOnAny(s_repositoryImplementations)
            .AndShould().NotDependOnAny(s_integrationImplementations)
            .AndShould().NotDependOnAny(s_persistence)
            .AndShould().NotDependOnAny(s_ravenDb)
            .Because("project-structure.md, Permitted dependencies: Validation");

        rule.Check(s_architecture);
    }

    [Fact]
    public void ApiAssembly_DoesNotUseImplementationsPersistenceOrRavenDb()
    {
        IArchRule rule = Types().That().Are(s_apiAssembly)
            .Should().NotDependOnAny(s_applicationServiceImplementations)
            .AndShould().NotDependOnAny(s_repositories)
            .AndShould().NotDependOnAny(s_integrations)
            .AndShould().NotDependOnAny(s_persistence)
            .AndShould().NotDependOnAny(s_ravenDb)
            .Because("project-structure.md, Permitted dependencies: API assembly");

        rule.Check(s_architecture);
    }

    [Fact]
    public void ApiAssembly_DoesNotUseDomainEntities()
    {
        IArchRule rule = Types().That().Are(s_apiAssembly)
            .Should().NotDependOnAny(s_entities)
            .Because("project-structure.md, Permitted dependencies: API assembly");

        rule.Check(s_architecture);
    }

    [Fact]
    public void ApplicationServiceAbstractions_DoNotUseDomainEntities()
    {
        IArchRule rule = Types().That().Are(s_applicationServiceAbstractions)
            .Should().NotDependOnAny(s_entities)
            .Because("project-structure.md, Permitted dependencies: ApplicationServices/Abstractions");

        rule.Check(s_architecture);
    }

    [Fact]
    public void Contracts_DoNotUseDomainEntities()
    {
        IArchRule rule = Types().That().Are(s_contracts)
            .Should().NotDependOnAny(s_entities)
            .Because("project-structure.md, Permitted dependencies: Contracts");

        rule.Check(s_architecture);
    }

    private static IObjectProvider<IType> CoreLayer(string relativeNamespace)
    {
        return Types().That().ResideInNamespaceOrBelow($"{CoreRoot}.{relativeNamespace}").As(relativeNamespace);
    }
}
