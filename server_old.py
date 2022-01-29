import logging
import asyncio
import grpc

from concurrent import futures

import localization.main as localize
import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc

logging.basicConfig(level=logging.NOTSET)


class Image(image_pb_grpc.ImageServicer):
    # byte_image = None

    async def SendImage(self, request_iterator, context):
        reqMetadata = dict(context.invocation_metadata())

        # TODO: make api key handler
        if (reqMetadata['x-graff-api-key'] == 'KpN4I5l4G8gFB8HVx6Xd'):
            async for image in request_iterator:
                logging.info(image.message)
                # logging.info(len(str(image.bytesImage)))
                # logging.info(type(image.bytesImage))

                gps_coor, utm_coor, pvec, qvec, tvec = localize.main(
                    image.bytesImage, image.cameraInfo, image.gpsPosition)

                user_world_coor = image_pb.WorldCoordinate(
                    latitude=gps_coor[0], longitude=gps_coor[1], altitude=0, utm_x=utm_coor[0], utm_y=utm_coor[1]
                )
                user_colmap_coor = image_pb.ColmapCoordinate(
                    qw=qvec[0], qx=qvec[1], qy=qvec[2], qz=qvec[3], tx=tvec[0], ty=tvec[1], tz=tvec[2], px=pvec[0], py=pvec[1], pz=pvec[2]
                )

                # user_position.latitude = latitude
                # user_position.longitude = longitude
                # user_position.altitude = altitude
                # self.byte_image = image.chunk_data

                logging.info("Localization Successful")
                # logging.info(user_world_coor)
                # logging.info(user_colmap_coor)

                yield image_pb.ImageResponse(
                    message='Localization Successful',
                    worldCoor=user_world_coor,
                    colmapCoor=user_colmap_coor
                )


async def main():
    logging.info('PYTHON gRPC IS RUNNING')
    server = grpc.aio.server(futures.ThreadPoolExecutor(max_workers=20))
    image_pb_grpc.add_ImageServicer_to_server(Image(), server)
    server.add_insecure_port('[::]:50052')
    await server.start()
    await server.wait_for_termination()


if __name__ == '__main__':
    asyncio.run(main())
