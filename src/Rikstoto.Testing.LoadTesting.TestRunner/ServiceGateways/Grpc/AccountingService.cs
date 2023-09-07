using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rikstoto.Grpc.Infrastructure;
using Rikstoto.Grpc.SharedContracts;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Accounting.Contracts.Protobuf;
using Rikstoto.Service.SharedContracts;
using Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using HealthCheckStatus = Rikstoto.Monitoring.HealthCheck.HealthCheckStatus;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public class AccountingService : IAccountingService
    {
        private readonly ILogger _logger;
        private VirtualAccountService.VirtualAccountServiceClient _service;

        public AccountingService(GrpcClientCreator grpcClientCreator, ILogger<AccountingService> logger)
        {
            _logger = logger;
            _service = grpcClientCreator.CreateClient<VirtualAccountService.VirtualAccountServiceClient>(ServiceAddresses.Accounting);
        }

        public async Task<CommandResult> Reserve(CustomerKey customer, Service.SharedContracts.Money amount, PurchaseId purchaseId, bool withoutExpiration, string transactionText = null)
        {
            var commandResult = await _service.ReserveAsync(new ReserveRequest
            {
                Customer = customer.ToProtobuf(),
                Amount = amount.ToProtobuf(),
                PurchaseId = purchaseId.Value.ToString().ToProtobuf(),
                WithoutExpiration = withoutExpiration,
                TransactionText = string.IsNullOrEmpty(transactionText) ? string.Empty.ToProtobuf() : transactionText.ToProtobuf()
            });
            return commandResult;
        }
    }
}