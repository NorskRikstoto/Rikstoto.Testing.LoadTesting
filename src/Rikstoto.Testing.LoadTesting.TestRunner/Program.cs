using System.Drawing;
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
var nrOfCores = 8.0; //check in Task Manager under the Performance tab
var nrOfRunsSetup = 20;
var nrOfRunsBets = 100;
var fileLocation = Environment.CurrentDirectory + "\\bets.txt";
var startingCustomerId = 8_000_001;

void SetupReservations()
{
    for (int i = 0; i <= nrOfRunsSetup - 1; i++)
    {
        var nrOfTickets = 50;
        var count = i * nrOfTickets;
        var numbers = Enumerable.Range(startingCustomerId + count, nrOfTickets);

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
            BetData.Parse("d:2023-01-18|t:BJ|g:V75|nt:1|w:1|org:NR|p:4900|pr:50|o:2|s1:10|s2:2|s3:46|s4:2|s5:2|s6:102|s7:22|l:1")
        };
            var totalCost = new Money
            {
                Amount = 5000
            };
            var agentKey = new AgentKey
            {
                AgentId = "00801"
            };

            var betLimitResult = betLimitsServiceClient.AddBetLimitReservation(customerId, totalCost, purchaseId).Result;
            if (betLimitResult.Success)
                Console.WriteLine($"customerId: {customerId} AddBetLimitReservation OK");
            else
                Console.WriteLine($"customerId: {customerId} AddBetLimitReservation failed: {betLimitResult.Message}", Color.Red);

            var registerTicketPurchaseResult = bettingServiceClient.RegisterTicketPurchaseDrafts(customerId, agentKey, purchaseId, betDataList, null).Result;
            if (registerTicketPurchaseResult.Success)
                Console.WriteLine($"customerId: {customerId} RegisterTicketPurchaseDrafts OK");
            else
                Console.WriteLine($"customerId: {customerId} RegisterTicketPurchaseDrafts failed: {registerTicketPurchaseResult.Message}", Color.Red);

            var reserveResult = accountServiceClient.Reserve(customerId, totalCost, purchaseId, false).Result;
            if (reserveResult.Success)
            {
                Console.WriteLine($"customerId: {customerId} Reserve OK");
                WriteFile($"{customerId}={purchaseId.Id}{Environment.NewLine}", fileLocation);
            }
            else
                Console.WriteLine($"customerId: {customerId} Reserve failed: {reserveResult.Message}", Color.Red);
        }
    }
}

void WriteFile(string fileContents, string filePathAndName)
{
    using (var mutex = new Mutex(false, filePathAndName.Replace("\\", "")))
    {
        var hasHandle = false;
        try
        {
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            File.AppendAllText(filePathAndName, fileContents);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (hasHandle)
                mutex.ReleaseMutex();
        }
    }
}

void DoBets(){
    var customerDictionary = new Dictionary<string, string>();
    void ParseToDictionary(string filePathName)
    {
        var lines = File.ReadLines(filePathName);
        foreach (var line in lines)
        {
            var values = line.Split('=');
            customerDictionary.Add(values[0], values[1]);
        }
    }

    ParseToDictionary(fileLocation);

    Thread.Sleep(3000);

    for (int i = 0; i <= nrOfRunsBets - 1; i++)
    {
        var nrOfTickets = 10;
        var count = i * nrOfTickets;
        var numbers = Enumerable.Range(startingCustomerId + count, nrOfTickets);

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
                Id = customerDictionary[customerId.ToString()]
            };

            var placeBetsResult = bettingServiceClient.PlaceBets(customerId, purchaseId).Result;
            if (placeBetsResult.Success)
                Console.WriteLine($"customerId: {customerId} PlaceBets OK");
            else
                Console.WriteLine($"customerId: {customerId} PlaceBets failed: {placeBetsResult.Message}", Color.Red);
        }
    }
    File.Delete(fileLocation);
}

//SetupReservations();
//DoBets();

await host.RunAsync();