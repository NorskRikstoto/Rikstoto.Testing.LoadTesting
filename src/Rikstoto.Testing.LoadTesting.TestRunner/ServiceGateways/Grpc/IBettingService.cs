using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rikstoto.Common.BetData.DomainModels;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Betting.Contracts.Protobuf;
using Rikstoto.Service.SharedContracts;
using AgentKey = Rikstoto.Service.SharedContracts.AgentKey;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using Money = Rikstoto.Service.SharedContracts.Money;
using TrackKey = Rikstoto.Service.SharedContracts.TrackKey;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;
using RaceDayKey = Rikstoto.Service.SharedContracts.RaceDayKey;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public interface IBettingService
    {
        Task<CommandResult> RegisterTicketPurchaseDrafts(CustomerKey customerKey, AgentKey agentId, PurchaseId purchaseIdentifier, IEnumerable<BetData> betDataList, TrackKey ownerTrack);
        Task<CommandResult> PlaceBets(CustomerKey customerKey, PurchaseId purchaseId);
    }
}