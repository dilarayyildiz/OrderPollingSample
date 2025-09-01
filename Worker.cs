namespace OrderPollingSample;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Services.OrderPollingService _poller;

    public Worker(ILogger<Worker> logger, Services.OrderPollingService poller)
    {
        _logger = logger;
        _poller = poller;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _poller.PollOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}