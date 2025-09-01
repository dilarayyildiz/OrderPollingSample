using System.Net;
using System.Net.Http.Headers;

namespace OrderPollingSample.Services;

public class OrderPollingService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenManager _tokenManager;
    private readonly ILogger<OrderPollingService> _logger;

    public OrderPollingService(IHttpClientFactory httpFactory, TokenManager tokenManager, ILogger<OrderPollingService> logger)
    {
        _httpFactory = httpFactory;
        _tokenManager = tokenManager;
        _logger = logger;
    }

    public async Task PollOnceAsync(CancellationToken ct)
    {
        var token = await _tokenManager.GetValidTokenAsync(ct);

        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = "https://jsonplaceholder.typicode.com/todos"; // test endpoint
        var resp = await client.GetAsync(url, ct);

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("401 received, forcing token refresh...");
            await _tokenManager.ForceRefreshAsync(ct);

            token = await _tokenManager.GetValidTokenAsync(ct);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            resp = await client.GetAsync(url, ct);
        }

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Orders API returned {code}", resp.StatusCode);
            return;
        }

        var body = await resp.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("Orders fetched: {len} bytes", body.Length);
    }
}