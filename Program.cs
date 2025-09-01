using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderPollingSample;
using OrderPollingSample.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<TokenManager>();
builder.Services.AddSingleton<OrderPollingService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();