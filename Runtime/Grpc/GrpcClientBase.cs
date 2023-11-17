using System;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace UnityEngine.Graffity.ARCloud
{
    public abstract class GrpcClientBase
    {
        protected SslCredentials sslCredentials;
        protected Channel grpcChannel;
        protected Metadata grpcHeader;
        protected string host;

        protected GrpcClientBase(string host, string apiKey)
        {
            this.host = host;
            grpcHeader = new Metadata()
            {
                {"x-graff-api-key", apiKey}
            };
        }

        public void ConnectServer(bool secure = true)
        {
            try
            {
                if (grpcChannel != null)
                {
                    DisposeServer();
                }

                if (secure)
                {
                    sslCredentials = new SslCredentials(null, null);
                    grpcChannel = new Channel(host, sslCredentials) { };
                }
                else
                {
                    grpcChannel = new Channel(host, ChannelCredentials.Insecure) { };
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Grpc Client SSL Connect Error: " + e);
            }
        }

        public void DisposeServer()
        {
            if (grpcChannel == null)
            {
                Debug.Log("Error: Client Not Connect");
                return;
            }

            grpcChannel.ShutdownAsync().Wait();
        }
#pragma warning disable CS1998
        public virtual object SendGrpc<TRequest>(TRequest info)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<object> SendGrpcAsync<TRequest>(TRequest info)
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1998

    }
}