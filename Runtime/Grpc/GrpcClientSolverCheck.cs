using System.Threading.Tasks;
using Solver;

namespace UnityEngine.Graffity.ARCloud
{
    public class GrpcClientSolverCheck: GrpcClientBase
    {
        public GrpcClientSolverCheck(string host, string apiKey) : base(host, apiKey)
        {
        }

        public override object SendGrpc<TRequest>(TRequest info)
        {
            var client = new StatusCheck.StatusCheckClient(grpcChannel);
            var reply = client.Check(info as Empty, headers:grpcHeader);
            return reply;
        }

        public override async Task<object> SendGrpcAsync<TRequest>(TRequest info)
        {
            var client = new StatusCheck.StatusCheckClient(grpcChannel);
            var reply = await client.CheckAsync(info as Empty, headers:grpcHeader);
            return reply;
        }
    }
}