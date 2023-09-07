using System.Threading.Tasks;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Betting.BetLimits.Contracts.Protobuf;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using Money = Rikstoto.Service.SharedContracts.Money;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public interface IBetLimitsService
    {
        Task<CommandResult> AddBetLimitReservation(CustomerKey customer, Money amount, PurchaseId purchaseId);
    }
}
