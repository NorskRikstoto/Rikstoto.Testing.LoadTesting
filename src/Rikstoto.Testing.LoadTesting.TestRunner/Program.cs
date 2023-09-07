using Rikstoto.Common.BetData.DomainModels;
using Rikstoto.Grpc.Infrastructure.Logging;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Toto.ServiceGateways.Grpc;
using Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<GrpcClientCreator>(new GrpcClientCreator(new ClientLoggingInterceptor(null), new Correlator(null)));
builder.Services.AddTransient<IBettingService, BettingService>();
builder.Services.AddTransient<IBetLimitsService, BetLimitsService>();
builder.Services.AddTransient<IAccountingService, AccountingService>();

using IHost host = builder.Build();
var nrOfCores = 8.0;

for (int i = 0; i <= 40;  i++)
{
    var startingCustomerId = 80000001;
    var nrOfTickets = 25;
    var count = i * nrOfTickets;
    var numbers = Enumerable.Range(startingCustomerId + i, Math.Max(count , nrOfTickets));

    Parallel.ForEach(numbers,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * nrOfCores))
                    },
                    RunFromCmdPromptAsync);

    void RunFromCmdPromptAsync(int customerId)
    {
        Console.WriteLine($"customerId: {customerId}");

        var hostProvider = host.Services;
        using IServiceScope serviceScope = hostProvider.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        var betLimitsServiceClient = provider.GetRequiredService<IBetLimitsService>();
        var bettingServiceClient = provider.GetRequiredService<IBettingService>();
        var accountServiceClient = provider.GetRequiredService<IAccountingService>();

        var purchaseId = new PurchaseId
        {
            Id = Guid.NewGuid().ToString()
        };
        var betDataList = new List<BetData>
        {
            BetData.Parse("d:2023-01-17|t:BJ|g:V75|nt:1|w:1|org:NR|p:50|pr:50|o:0|s1:2|s2:2048|s3:44|s4:524|s5:576|s6:32|s7:146")
        };
        var totalCost = new Money
        {
            Amount = 5600
        };
        var agentKey = new AgentKey
        {
            AgentId = "00801"
        };

        var betLimitResult = betLimitsServiceClient.AddBetLimitReservation(customerId, totalCost, purchaseId).Result;
        Console.WriteLine($"AddBetLimitReservation: {betLimitResult}");
        var registerTicketPurchaseResult = bettingServiceClient.RegisterTicketPurchaseDrafts(customerId, agentKey, purchaseId, betDataList, null).Result;
        Console.WriteLine($"RegisterTicketPurchaseDrafts: {registerTicketPurchaseResult}");
        var reserveResult = accountServiceClient.Reserve(customerId, totalCost, purchaseId, false).Result;
        Console.WriteLine($"Reserve: {reserveResult}");
        var placeBetsResult = bettingServiceClient.PlaceBets(customerId, purchaseId).Result;
        Console.WriteLine($"PlaceBets: {placeBetsResult}");
    }

    Thread.Sleep(1000);
}

await host.RunAsync();