using System;
using Elastic.Apm;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Rikstoto.Grpc.Infrastructure.Logging;

namespace Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure
{
    public class SetRikstotoCorrelationIdInterceptor : Interceptor
    {
        private readonly ICorrelator _correlator;

        public SetRikstotoCorrelationIdInterceptor(ICorrelator correlator)
        {
            _correlator = correlator;
        }
        
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata();
            if (Agent.IsConfigured && Agent.Tracer.CurrentTransaction?.OutgoingDistributedTracingData != null)
            {
                var serializedTraceData = Agent.Tracer.CurrentTransaction.OutgoingDistributedTracingData.SerializeToString();
                headers.Add(new Metadata.Entry(LogConstants.ElasticApmTraceData, serializedTraceData));
            }
            
            var correlationId = _correlator.CurrentCorrelationId ?? Guid.Empty;
            if(correlationId != Guid.Empty)
                headers.Add(new Metadata.Entry(LogConstants.CorrelationIdHeader, correlationId.ToString("D")));

            if(headers.Count == 0)
                return base.BlockingUnaryCall(request, context, continuation);
            
            var optionsWithHeaders = context.Options.WithHeaders(headers);
            var contextWithHeaders = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, optionsWithHeaders);
            
            return base.BlockingUnaryCall(request, contextWithHeaders, continuation);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var headers = new Metadata();
            if (Agent.IsConfigured && Agent.Tracer.CurrentTransaction?.OutgoingDistributedTracingData != null)
            {
                var serializedTraceData = Agent.Tracer.CurrentTransaction.OutgoingDistributedTracingData.SerializeToString();
                headers.Add(new Metadata.Entry(LogConstants.ElasticApmTraceData, serializedTraceData));
            }
            
            var correlationId = _correlator.CurrentCorrelationId ?? Guid.Empty;
            if(correlationId != Guid.Empty)
                headers.Add(new Metadata.Entry(LogConstants.CorrelationIdHeader, correlationId.ToString("D")));

            if (headers.Count == 0)
                return base.AsyncUnaryCall(request, context, continuation);
                
            var optionsWithHeaders = context.Options.WithHeaders(headers);
            
            var contextWithHeaders = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, optionsWithHeaders);
            return base.AsyncUnaryCall(request, contextWithHeaders, continuation);
        }
    }
}