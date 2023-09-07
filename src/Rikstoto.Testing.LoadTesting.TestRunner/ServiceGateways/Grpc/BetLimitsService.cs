using System.Threading.Tasks;
using Rikstoto.Grpc.Infrastructure;
using Rikstoto.Grpc.SharedContracts;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Betting.BetLimits.Contracts.Protobuf;
using Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using Money = Rikstoto.Service.SharedContracts.Money;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public class BetLimitsService : IBetLimitsService
    {
        private readonly Service.Betting.BetLimits.Contracts.Protobuf.BetLimitsService.BetLimitsServiceClient _service;

        public BetLimitsService(GrpcClientCreator grpcClientCreator)
        {
            _service = grpcClientCreator.CreateClient<Service.Betting.BetLimits.Contracts.Protobuf.BetLimitsService.BetLimitsServiceClient>(ServiceAddresses.Betting);
        }

        public async Task<CommandResult> AddBetLimitReservation(CustomerKey customer, Money amount, PurchaseId purchaseId)
        {
            AddBetLimitReservationRequest request = new AddBetLimitReservationRequest
            {
                Customer = customer,
                Amount = amount,
                PurchaseId = purchaseId.Value.ToString().ToProtobuf()
            };
            var response = await _service.AddBetLimitReservationAsync(request);
            return response;
        }
    }
}
