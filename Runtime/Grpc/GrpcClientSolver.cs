using System;
using System.Threading.Tasks;
using Grpc.Core;
using Solver;
using UnityEngine;

namespace UnityEngine.Graffity.ARCloud
{
    public class GrpcClientSolver: GrpcClientBase
    {
        public GrpcClientSolver(string host, string apiKey) : base(host, apiKey)
        {
        }

        public override object SendGrpc<TRequest>(TRequest info)
        {
            var client = new Solver.Solver.SolverClient(grpcChannel);
            var reply = client.Solve(info as SolveRequest, headers:grpcHeader);
            return reply;
        }

        public override async Task<object> SendGrpcAsync<TRequest>(TRequest info)
        {
            var client = new Solver.Solver.SolverClient(grpcChannel);
            var reply = await client.SolveAsync(info as SolveRequest, headers: grpcHeader);
            return reply;
        }
    }
}