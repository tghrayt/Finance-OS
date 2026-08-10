namespace FinanceOS.Foundation.Tests;

public sealed class RepositoryFoundationTests
{
    [Theory]
    [InlineData("FinanceOS.slnx")]
    [InlineData("backend/gateway/FinanceOS.Gateway/FinanceOS.Gateway.csproj")]
    [InlineData("backend/services/identity/FinanceOS.Identity.Api/FinanceOS.Identity.Api.csproj")]
    [InlineData("backend/services/finance/FinanceOS.Finance.Api/FinanceOS.Finance.Api.csproj")]
    [InlineData("backend/services/budget/FinanceOS.Budget.Api/FinanceOS.Budget.Api.csproj")]
    [InlineData("backend/services/forecast/FinanceOS.Forecast.Api/FinanceOS.Forecast.Api.csproj")]
    [InlineData("backend/services/notification/FinanceOS.Notification.Api/FinanceOS.Notification.Api.csproj")]
    [InlineData("apps/web/package.json")]
    [InlineData("apps/web/Dockerfile")]
    [InlineData("apps/web/nginx.conf")]
    [InlineData("docker-compose.yml")]
    [InlineData("docs/DEPLOYMENT.md")]
    [InlineData(".github/workflows/deploy-k3s.yml")]
    [InlineData("infrastructure/k8s/overlays/production/kustomization.yaml")]
    [InlineData("infrastructure/k8s/overlays/production/ingress.yaml")]
    [InlineData("infrastructure/k8s/overlays/production/ingress.example.yaml")]
    public void PhaseZeroFoundationFilesExist(string relativePath)
    {
        var repositoryRoot = FindRepositoryRoot();
        var path = Path.Combine(repositoryRoot.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(path), $"Expected foundation file to exist: {relativePath}");
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FinanceOS.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
