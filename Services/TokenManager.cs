namespace OrderPollingSample.Services;

public class TokenManager
{
    private readonly object _lock = new();
    private string? _accessToken;
    private DateTimeOffset _expiresAtUtc;
    private readonly Queue<DateTimeOffset> _requests = new();
    private readonly ILogger<TokenManager> _logger;

    public TokenManager(ILogger<TokenManager> logger) => _logger = logger;

    public async Task<string> GetValidTokenAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAtUtc - now > TimeSpan.FromMinutes(2))
            return _accessToken!;

        if (!CanRequestNewToken())
            throw new InvalidOperationException("Token request limit reached (5/hour).");

        RegisterTokenRequest();

        await Task.Delay(50, ct); // simülasyon
        _accessToken = Guid.NewGuid().ToString("N");
        _expiresAtUtc = now.AddHours(1);

        _logger.LogInformation("New token requested. Expires at {exp:u}", _expiresAtUtc);
        return _accessToken!;
    }

    public async Task ForceRefreshAsync(CancellationToken ct)
    {
        if (!CanRequestNewToken())
            throw new InvalidOperationException("Token request limit reached (5/hour).");

        RegisterTokenRequest();

        await Task.Delay(50, ct);
        _accessToken = Guid.NewGuid().ToString("N");
        _expiresAtUtc = DateTimeOffset.UtcNow.AddHours(1);

        _logger.LogInformation("Token forced refresh. Expires at {exp:u}", _expiresAtUtc);
    }

    private bool CanRequestNewToken()
    {
        var now = DateTimeOffset.UtcNow;
        lock (_lock)
        {
            while (_requests.Count > 0 && now - _requests.Peek() > TimeSpan.FromHours(1))
                _requests.Dequeue();

            return _requests.Count < 5;
        }
    }

    private void RegisterTokenRequest()
    {
        lock (_lock) { _requests.Enqueue(DateTimeOffset.UtcNow); }
    }
}