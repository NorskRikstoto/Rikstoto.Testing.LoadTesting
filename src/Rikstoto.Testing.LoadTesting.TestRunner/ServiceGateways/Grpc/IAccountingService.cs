using System;
using System.Threading.Tasks;
using Rikstoto.Grpc.SharedContracts.Protobuf;
using Rikstoto.Service.Accounting.Contracts.Protobuf;
using Rikstoto.Service.SharedContracts;
using CustomerKey = Rikstoto.Service.SharedContracts.CustomerKey;
using HealthCheckStatus = Rikstoto.Monitoring.HealthCheck.HealthCheckStatus;
using PurchaseId = Rikstoto.Service.SharedContracts.PurchaseId;

namespace Rikstoto.Toto.ServiceGateways.Grpc
{
    public interface IAccountingService
    {
        Task<CommandResult> Reserve(CustomerKey customer, Service.SharedContracts.Money amount, PurchaseId purchaseIdentifier, bool withoutExpiration, string transactionText = null);
    }
}