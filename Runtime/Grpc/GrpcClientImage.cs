using System;
using System.Threading.Tasks;
using Vpsimage;

namespace UnityEngine.Graffity.ARCloud
{
    public class GrpcClientImage : GrpcClientBase
    {
        private Action<ImageResponse> callBackActionReply;
        public GrpcClientImage(string host, string apiKey) : base(host, apiKey)
        {
        }

        public override async Task<object> SendGrpcAsync<TRequest>(TRequest info)
        {
            var client = new Vpsimage.Image.ImageClient(grpcChannel);
            var reply = await client.SendImageAsync(info as ImageRequest, headers: grpcHeader, deadline: DateTime.UtcNow.AddSeconds(300));
            return reply;
        }
    }
}