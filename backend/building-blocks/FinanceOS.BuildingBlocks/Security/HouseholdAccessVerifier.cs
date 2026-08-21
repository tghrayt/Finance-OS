using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FinanceOS.BuildingBlocks.Security;

public interface IHouseholdAccessVerifier
{
    Task<bool> CanAccessHouseholdAsync(Guid householdId, HttpContext httpContext, CancellationToken cancellationToken);
}

internal sealed class HouseholdAccessVerifier(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IWebHostEnvironment environment) : IHouseholdAccessVerifier
{
    private const string CurrentHouseholdCacheKey = "FinanceOS.CurrentHouseholdId";

    public async Task<bool> CanAccessHouseholdAsync(
        Guid householdId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!configuration.ShouldRequireFinanceOSAuthorization(environment))
        {
            return true;
        }

        if (householdId == Guid.Empty)
        {
            return false;
        }

        var currentHouseholdId = await GetCurrentHouseholdIdAsync(httpContext, cancellationToken);

        return currentHouseholdId == householdId;
    }

    private async Task<Guid?> GetCurrentHouseholdIdAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (httpContext.Items.TryGetValue(CurrentHouseholdCacheKey, out var cachedValue)
            && cachedValue is Guid cachedHouseholdId)
        {
            return cachedHouseholdId;
        }

        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)
            || !AuthenticationHeaderValue.TryParse(authorization, out var authorizationHeader))
        {
            return null;
        }

        var identityBaseUrl = configuration["Identity:BaseUrl"];
        if (string.IsNullOrWhiteSpace(identityBaseUrl))
        {
            return null;
        }

        var client = httpClientFactory.CreateClient("FinanceOS.Identity");
        client.BaseAddress = new Uri(identityBaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Authorization = authorizationHeader;

        using var response = await client.GetAsync("api/v1/identity/households/current", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var household = await JsonSerializer.DeserializeAsync<CurrentHouseholdResponse>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web),
            cancellationToken);

        if (household?.HouseholdId is not { } householdId || householdId == Guid.Empty)
        {
            return null;
        }

        httpContext.Items[CurrentHouseholdCacheKey] = householdId;

        return householdId;
    }

    private sealed record CurrentHouseholdResponse(Guid? HouseholdId);
}
