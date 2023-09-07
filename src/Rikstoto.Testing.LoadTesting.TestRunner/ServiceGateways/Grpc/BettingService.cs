using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rikstoto.Common.BetData.DomainModels;
using Rikstoto.Grpc.Infrastructure;
using Rikstoto.Grpc.SharedContracts;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Betting.Contracts.Protobuf;
using Rikstoto.Service.SharedContracts;
using Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure;
using AgentKey = Rikstoto.Service.SharedContracts.AgentKey;
using BetMethod = Rikstoto.Service.Betting.Contracts.Protobuf.BetMethod;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using Money = Rikstoto.Service.SharedContracts.Money;
using TrackKey = Rikstoto.Service.SharedContracts.TrackKey;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;
using RaceDayKey = Rikstoto.Service.SharedContracts.RaceDayKey;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public class BettingService : IBettingService
    {
        private readonly ILogger _logger;
        private readonly Service.Betting.Contracts.Protobuf.BettingService.BettingServiceClient _service;

        public BettingService(GrpcClientCreator grpcClientCreator, ILogger<BettingService> logger)
        {
            _logger = logger;
            _service = grpcClientCreator.CreateClient<Service.Betting.Contracts.Protobuf.BettingService.BettingServiceClient>(ServiceAddresses.Betting);
        }

        public async Task<CommandResult> RegisterTicketPurchaseDrafts(CustomerKey customerKey, AgentKey agentId, PurchaseId purchaseId, IEnumerable<BetData> betDataList, TrackKey trackKey)
        {
            var registerTicketPurchaseDraftsRequest = new RegisterTicketPurchaseDraftsRequest
            {
                Customer = customerKey,
                AgentKey = agentId,
                PurchaseId = purchaseId.ToString().ToProtobuf(),
                BetDataStrings =
                {
                    betDataList.Select(betData => betData.SerializeToBetDataString().ToProtobuf()).ToProtobufRepeated()
                },
                OwnerTrack = trackKey ?? new Rikstoto.Grpc.SharedContracts.Protobuf.TrackKey()
            };
            var response = await _service.RegisterTicketPurchaseDraftsAsync(registerTicketPurchaseDraftsRequest);
            
            return response;
        }

        public async Task<CommandResult> PlaceBets(CustomerKey customerKey, PurchaseId purchaseId)
        {
            var response = await _service.PlaceBetsAsync(new PlaceBetsRequest
            {
                Customer = customerKey,
                PurchaseId = purchaseId.ToString().ToProtobuf()
            });
            
            return response;
        }
    }
}