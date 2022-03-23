import grpc

import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc


host = 'vps.graffity.services'


def main():
    credentials = grpc.ssl_channel_credentials(
        root_certificates=None, private_key=None, certificate_chain=None)

    with grpc.secure_channel(host, credentials) as channel:
        stub = image_pb_grpc.AvailableAreaStub(channel)

        metadata = [
            ('x-graff-api-key', 'KpN4I5l4G8gFB8HVx6Xd')
        ]

        # Bank Room
        gpsPosition = image_pb.Position(
            latitude=13.579003,
            longitude=100.109626,
            altitude=0.0
        )

        # inside TDPK
        # gpsPosition = image_pb.Position(
        #     latitude=13.685685,
        #     longitude=100.611000,
        #     altitude=0.0
        # )

        # inside OneSiam Skywalk
        # gpsPosition = image_pb.Position(
        #     latitude=13.746103,
        #     longitude=100.530739,
        #     altitude=0.0
        # )

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
