using System.Drawing;
using Rikstoto.Common.BetData.DomainModels;
using Rikstoto.Grpc.Infrastructure.Logging;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Toto.ServiceGateways.Grpc;
using Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Rikstoto.Testing.LoadTesting.TestRunner;
using Microsoft.Extensions.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<GrpcClientCreator>(new GrpcClientCreator(new ClientLoggingInterceptor(null), new Correlator(null)));
builder.Services.AddTransient<IBettingService, BettingService>();
builder.Services.AddTransient<IBetLimitsService, BetLimitsService>();
builder.Services.AddTransient<IAccountingService, AccountingService>();
builder.Services.Configure<TestRunSetup>(builder.Configuration.GetSection(nameof(TestRunSetup)));

using IHost host = builder.Build();
var testRunSetup = host.Services.GetRequiredService<IOptions<TestRunSetup>>().Value;
var nrOfCores = 8.0; //check in Task Manager under the Performance tab
var fileLocation = Environment.CurrentDirectory + "\\bets.txt";

void SetupReservations()
{
    var betLimitReservationSuccess = 0;
    var betLimitReservationFailure = 0;
    var registerDraftSuccess = 0;
    var registerDraftFailure = 0;
    var reserverSuccess = 0;
    var reserverFailure = 0;

    var stopWatch = Stopwatch.StartNew();
    File.Delete(fileLocation);
    for (int i = 0; i <= testRunSetup.NrOfRunsForReservations - 1; i++)
    {
        var count = i * testRunSetup.NrOfReservationsPerRun;
        var numbers = Enumerable.Range(testRunSetup.StartingCustomerId + count, testRunSetup.NrOfReservationsPerRun);

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
                BetData.Parse(testRunSetup.BetDataString)
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
            Log(customerId, betLimitResult.Success, nameof(betLimitsServiceClient.AddBetLimitReservation), betLimitResult.Message, ref betLimitReservationSuccess, ref betLimitReservationFailure);
                

            var registerTicketPurchaseResult = bettingServiceClient.RegisterTicketPurchaseDrafts(customerId, agentKey, purchaseId, betDataList, null).Result;
            Log(customerId, registerTicketPurchaseResult.Success, nameof(bettingServiceClient.RegisterTicketPurchaseDrafts), registerTicketPurchaseResult.Message, ref registerDraftSuccess, ref registerDraftFailure);

            var reserveResult = accountServiceClient.Reserve(customerId, totalCost, purchaseId, false).Result;
            if (reserveResult.Success)
            {
                Console.WriteLine($"customerId: {customerId} Reserve OK");
                WriteToFile($"{customerId}={purchaseId.Id}{Environment.NewLine}", fileLocation);
                reserverSuccess++;
            }
            else
            {
                Console.WriteLine($"customerId: {customerId} Reserve failed: {reserveResult.Message}", Color.Red);
                reserverFailure++;
            }
        }
    }

    stopWatch.Stop();
    var ts = stopWatch.Elapsed;

    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
    Console.WriteLine("RunTime " + elapsedTime);
    var totalReservations = testRunSetup.NrOfReservationsPerRun * testRunSetup.NrOfRunsForReservations;
    Console.WriteLine($"AddBetLimitReservation{Environment.NewLine} t:{totalReservations} s:{betLimitReservationSuccess} f:{betLimitReservationFailure}");
    Console.WriteLine($"RegisterTicketPurchaseDrafts{Environment.NewLine} t:{totalReservations} s:{registerDraftSuccess} f:{registerDraftFailure}");
    Console.WriteLine($"Reserve{Environment.NewLine} t:{totalReservations} s: {reserverSuccess} f:{reserverFailure}");
}

void DoBets(){
    var stopWatch = Stopwatch.StartNew();

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

    for (int i = 0; i <= testRunSetup.NrOfRunsForBets - 1; i++)
    {
        var count = i * testRunSetup.NrOfBetsPerRun;
        var numbers = Enumerable.Range(testRunSetup.StartingCustomerId + count, testRunSetup.NrOfBetsPerRun);

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

    stopWatch.Stop();
    var ts = stopWatch.Elapsed;

    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
    Console.WriteLine("RunTime " + elapsedTime);
}

//SetupReservations();
//DoBets();

await host.RunAsync();

void WriteToFile(string fileContents, string filePathAndName)
{
    using var mutex = new Mutex(false, filePathAndName.Replace("\\", ""));
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

void Log(int customerId, bool success, string actionName, string message, ref int successCount, ref int failerCount)
{
    if (success)
    {
        Console.WriteLine($"customerId: {customerId} {actionName} OK");
        successCount++;
    }
    else
    {
        Console.WriteLine($"customerId: {customerId} {actionName} failed: {message}", Color.Red);
        failerCount++;
    }
}