using System;
using System.Threading.Tasks;
using Grpc.Core;
using Vpsimage;
using Solver;
using UnityEngine;
using UnityEngine.Graffity.ARCloud;
using static UnityEngine.Graffity.ARCloud.ARCloudConstant;

namespace UnityEngine.Graffity.ARCloud
{
    public class VpsGrpcManager
    {
        //TODO not expose image client or refractor vpsGrpcManager
        private GrpcClientAvailableArea availableAreaClient;
        public GrpcClientImage imageClient;
        private GrpcClientSolver solverClient;
        private GrpcClientSolverCheck solverCheckClient;
        private ApiCredConfig apiCredConfig;

        public VpsGrpcManager(ApiCredConfig apiCredConfig)
        {
            this.apiCredConfig = apiCredConfig;
            availableAreaClient = new GrpcClientAvailableArea(VpsGrpcConstant.IMAGE_HOST_NAME, apiCredConfig.consoleAccessToken);
            imageClient = new GrpcClientImage(VpsGrpcConstant.IMAGE_HOST_NAME, apiCredConfig.consoleAccessToken);
            // #if UNITY_EDITOR
            //             solverClient = new GrpcClientSolver("localhost", apiCredConfig.graffApiKey);
            //             solverCheckClient = new GrpcClientSolverCheck("localhost", apiCredConfig.graffApiKey);
            // #elif UNITY_ANDROID && !UNITY_EDITOR
            //             solverClient = new GrpcClientSolver("192.168.1.101", apiCredConfig.graffApiKey);
            //             solverCheckClient = new GrpcClientSolverCheck("192.168.1.101", apiCredConfig.graffApiKey);
            // #else
            var solverHost = apiCredConfig._useCustomSolverServer
                ? apiCredConfig._customSolverHost
                : VpsGrpcConstant.SOLVER_HOST_NAME;


            if (apiCredConfig._useCustomSolverServer)
                Debug.LogWarning($"Use custom solver host: {apiCredConfig._customSolverHost}");

            solverClient = new GrpcClientSolver(solverHost, apiCredConfig.consoleAccessToken);
            solverCheckClient = new GrpcClientSolverCheck(solverHost, apiCredConfig.consoleAccessToken);
            // #endif
        }

        public void ConnectServers()
        {
            availableAreaClient.ConnectServer();
            imageClient.ConnectServer();

            var isSolverConnectionSecure = !apiCredConfig._useCustomSolverServer || apiCredConfig._isSecure;

            solverClient.ConnectServer(secure: isSolverConnectionSecure);
            solverCheckClient.ConnectServer(secure: isSolverConnectionSecure);
        }

        public async Task<bool> ValidateConnections()
        {
            var checkImageTask = CheckAvailableAreaAsync(new AvailableAreaRequest()
            {
                GpsPosition = (Position)VALIDATE_CONNECTION_POSITION_GPS,
                MinDistance = 0,
                MaxDistance = 500
            }, (response) =>
        {
            if (!response.IsAvailable)
                throw new ARCloudException("Image server validate response is not valid");
            Debug.Log("Image server validate response is valid");
        }, (err) =>
            {
                Debug.Log("Image server validate response is not valid");
                // throw new ARCloudException("Image server validate response is not valid");
            }
                );
            var checkSolverTask = SolverStatusCheckAsync(new Empty(),
                (response) =>
                {
                    if (!response.Status)
                        throw new ARCloudException("Solver server validate response is not valid");
                    Debug.Log("Solver server validate response is valid");
                }, (err) =>
                {
                    Debug.Log("Solver server validate response is not valid");
                    throw new ARCloudException("Solver server validate response is not valid");
                });
            try
            {
                await Task.WhenAll(checkImageTask, checkSolverTask);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void ShutdownServers()
        {
            availableAreaClient.DisposeServer();
            imageClient.DisposeServer();
            solverClient.DisposeServer();
            solverCheckClient.DisposeServer();
        }

        public async Task<SolveResponse> RequestSolveAsync(SolveRequest info)
        {
            if (info.ArCoordinate.Count != info.VpsCoordinate.Count)
                throw new ARCloudException("Length of ArCoordinate points not equal length of VpsCoordinate points");
            try
            {
                var res = await solverClient.SendGrpcAsync(info) as SolveResponse;
                return res;
            }
            catch (Exception e)
            {
                // TODO retry
                Debug.Log(e);
                return null;
            }
        }


        public async Task SolverStatusCheckAsync(Empty info,
            Action<CheckResponse> onSuccessCb,
            Action<Exception> onErrorCb = null)
        {
            try
            {
                var response = await solverCheckClient.SendGrpcAsync(info);
                onSuccessCb(response as CheckResponse);
            }
            catch (RpcException rpcException)
            {
                onErrorCb?.Invoke(rpcException);
            }
            catch (Exception e)
            {
                onErrorCb?.Invoke(e);
            }
        }

        public AvailableAreaResponse CheckAvailableArea(AvailableAreaRequest info)
        {
            var response = availableAreaClient.SendGrpc(info);
            return response as AvailableAreaResponse;
        }

        public async Task<AvailableAreaResponse> CheckAvailableAreaAsync(AvailableAreaRequest info)
        {
            var response = await availableAreaClient.SendGrpcAsync(info);
            return response as AvailableAreaResponse;
        }

        public async Task CheckAvailableAreaAsync(AvailableAreaRequest info,
            Action<AvailableAreaResponse> onSuccessCb,
            Action<Exception> onErrorCb = null)
        {
            try
            {
                var response = await availableAreaClient.SendGrpcAsync(info);
                onSuccessCb(response as AvailableAreaResponse);
            }
            catch (RpcException rpcException)
            {
                onErrorCb?.Invoke(rpcException);
                throw;
            }
            catch (Exception e)
            {
                onErrorCb?.Invoke(e);
                throw;
            }
        }

        public async Task SendImageAsync(ImageRequest info,
            Action<ImageResponse> onSuccessCb,
            Action<Exception> onErrorCb = null)
        {
            try
            {
                var response = await imageClient.SendGrpcAsync(info);
                onSuccessCb(response as ImageResponse);
            }
            catch (RpcException rpcException)
            {
                onErrorCb?.Invoke(rpcException);
            }
            catch (Exception e)
            {
                onErrorCb?.Invoke(e);
            }
        }

    }
}