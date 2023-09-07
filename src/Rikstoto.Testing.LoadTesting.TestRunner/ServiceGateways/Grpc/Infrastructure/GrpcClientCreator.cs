using System;
using System.Collections.Concurrent;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Rikstoto.Grpc.Infrastructure.Logging;

namespace Rikstoto.Toto.ServiceGateways.Grpc.Infrastructure
{
    public class GrpcClientCreator
    {
        private readonly ClientLoggingInterceptor _loggingInterceptor;
        private readonly ICorrelator _correlator;
        private static readonly ConcurrentDictionary<string, CallInvoker> _channels = new ConcurrentDictionary<string, CallInvoker>();

        public GrpcClientCreator(ClientLoggingInterceptor loggingInterceptor, ICorrelator correlator)
        {
            _loggingInterceptor = loggingInterceptor;
            _correlator = correlator;
        }

        public T CreateClient<T>(string endpointAddress) where T : ClientBase
        {
            if (endpointAddress == null)
                throw new Exception("GRPC endpoint address cannot be null. Remembered to run 'r22 devenv set test2'?");

            var apmSpanInterceptor = new ExternalGrpcCallApmSpanInterceptor();
            var channelName = typeof(T).Name;
            if (!_channels.ContainsKey(channelName))
            {
                var channel = new Channel(endpointAddress, ChannelCredentials.Insecure);
                var interceptingInvoker = channel.Intercept(new SetRikstotoCorrelationIdInterceptor(_correlator), _loggingInterceptor, apmSpanInterceptor);

                var isInserted = false;
                do
                {
                    isInserted = _channels.TryAdd(channelName, interceptingInvoker);
                    if(!isInserted)
                        isInserted = _channels.ContainsKey(channelName);
                    
                } while (!isInserted);
            }

            var invoker = _channels[channelName];
            
            return (T) Activator.CreateInstance(typeof(T), invoker);
        }
    }
}