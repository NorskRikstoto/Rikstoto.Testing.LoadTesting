using Elastic.Apm;
using Elastic.Apm.Api;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure
{
    public class ExternalGrpcCallApmSpanInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            ISpan currentSpan = null;
            
            if (Agent.IsConfigured)
                currentSpan = Agent.Tracer.CurrentTransaction.StartSpan("external-call", "external-grpc-call");

            var asyncUnaryCall = base.AsyncUnaryCall(request, context, continuation);

            if (Agent.IsConfigured && currentSpan != null)
                currentSpan.End();
            
            return asyncUnaryCall;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            ISpan currentSpan = null;
            
            if (Agent.IsConfigured)
                currentSpan = Agent.Tracer.CurrentTransaction.StartSpan("external-call", "external-grpc-call");

            var blockingUnaryCall = base.BlockingUnaryCall(request, context, continuation);

            if (Agent.IsConfigured && currentSpan != null)
                currentSpan.End();
            
            return blockingUnaryCall;
        }
    }
}