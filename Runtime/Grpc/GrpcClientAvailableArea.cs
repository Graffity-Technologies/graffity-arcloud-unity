using System;
using System.Threading.Tasks;
using Grpc.Core;
using Image;

namespace UnityEngine.Graffity.ARCloud
{
    public class GrpcClientAvailableArea: GrpcClientBase
    {
        public GrpcClientAvailableArea(string host, string apiKey) : base(host, apiKey)
        {
        }

        public override object SendGrpc<TRequest>(TRequest info)
        {
            var client = new AvailableArea.AvailableAreaClient(grpcChannel);
            var reply = client.CheckAvailableArea(info as AvailableAreaRequest, headers:grpcHeader, deadline: DateTime.UtcNow.AddSeconds(5));
            return reply;
        }

        public override async Task<object> SendGrpcAsync<TRequest>(TRequest info)
        {
            var client = new AvailableArea.AvailableAreaClient(grpcChannel);
            var reply = await client.CheckAvailableAreaAsync(info as AvailableAreaRequest, headers: grpcHeader);
            return reply;
        }
    }
}