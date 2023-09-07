using System;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure
{
    public class Correlator : ICorrelator
    {
        private Guid _correlationId;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Correlator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        private const string RequestCorrelationIdKey = "RikstotoCorrelationId";

        public IDisposable BeginRequestCorrelationScope()
        {
            var correlationId = Guid.NewGuid();
            var logContext = LogContext.PushProperty("CorrelationId", correlationId);
            //_httpContextAccessor.HttpContext.Items.Add(RequestCorrelationIdKey, correlationId);
            _correlationId = correlationId;
            return new CorrelationScope(logContext);
        }

        public Guid? CurrentCorrelationId
        {
            get
            {
                //if (_httpContextAccessor.HttpContext == null)
                //    return null;
                
                //if (!_httpContextAccessor.HttpContext.Items.ContainsKey(RequestCorrelationIdKey))
                //    return null;

                //return (Guid)_httpContextAccessor.HttpContext.Items[RequestCorrelationIdKey];

                return _correlationId;
            }
        }

        private class CorrelationScope : IDisposable
        {
            private readonly IDisposable _logContext;

            public CorrelationScope(IDisposable logContext)
            {
                _logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
            }

            public void Dispose() => _logContext.Dispose();
        }
    }

    public interface ICorrelator
    {
        IDisposable BeginRequestCorrelationScope(); 
        Guid? CurrentCorrelationId { get; }
    }    
}