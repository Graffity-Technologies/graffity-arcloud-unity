import grpc

import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc


host = 'localhost:50052'


def main():
    with grpc.insecure_channel(host) as channel:
        stub = image_pb_grpc.AvailableAreaStub(channel)

        metadata = [
            ('x-graff-api-key', 'KpN4I5l4G8gFB8HVx6Xd')
        ]

        # inside TDPK
        # gpsPosition = image_pb.Position(
        #     latitude=13.685685,
        #     longitude=100.611000,
        #     altitude=0.0
        # )

        # inside OneSiam Skywalk
        gpsPosition = image_pb.Position(
            latitude=13.746103,
            longitude=100.530739,
            altitude=0.0
        )

        response = stub.CheckAvailableArea(
            image_pb.AvailableAreaRequest(
                gpsPosition=gpsPosition,
                maxDistance=500,
                minDistance=0
            ),
            metadata=metadata
        )

        print("Client received message: ", response.message)
        print(response.isAvailable)


if __name__ == '__main__':
    main()
