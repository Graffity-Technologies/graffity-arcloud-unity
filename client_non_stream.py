from __future__ import print_function
import time
import cv2
import grpc
import glob
import os

import protos.image_non_stream.image_non_stream_pb2 as image_pb
import protos.image_non_stream.image_non_stream_pb2_grpc as image_pb_grpc

from google.protobuf.json_format import MessageToDict
from argparse import ArgumentParser

host = 'localhost:50052'


def main(
    num_images: int,
    image_path: str,
    image_type: str,
    is_halfsize: bool,
    divider: int,
):
    res = []
    start_time_total = time.time()

    with grpc.insecure_channel(host) as channel:
        stub = image_pb_grpc.ImageStub(channel)

        metadata = [
            ('x-graff-api-key', 'KpN4I5l4G8gFB8HVx6Xd')
        ]

        im = cv2.imread(image_path)
        is_success, im_buf_arr = cv2.imencode(".jpg", im)
        byte_im = im_buf_arr.tobytes()

        cameraInfo = image_pb.CameraInfo(
            pixelFocalLength=3000/2 if is_halfsize else 3000/divider,
            principalPointX=2000/2 if is_halfsize else 2000/divider,
            principalPointY=1500/2 if is_halfsize else 1500/divider,
            radialDistortion=0/divider
        )

        # inside TDPK
        gpsPosition = image_pb.Position(
            latitude=13.685685,
            longitude=100.611000,
            altitude=0.0
        )

        msg = image_pb.ImageRequest(
            message='request-image-' + os.path.basename(image_path),
            bytesImage=byte_im,
            cameraInfo=cameraInfo,
            gpsPosition=gpsPosition
        )

        response = stub.SendImage(
            msg,
            metadata=metadata
        )

    # for response in responses:
    print("Client received message: ", response.message)
    # print("Client received worldCoor: ", response.worldCoor)
    # print("Client received colmapCoor: ", response.colmapCoor)
    # res.append(MessageToDict(response.colmapCoor))

    print('---------------------------------------')
    print("Total Responses Time:", (time.time() - start_time_total), "seconds")

    return response


if __name__ == '__main__':
    parser = ArgumentParser()
    parser.add_argument(
        '--num_images',
        type=int,
        default=1,
        help='Number of Images to Process',
    )

    parser.add_argument(
        '--image_path',
        type=str,
        default='images/TDPK/IMG_2838/4/1080/IMG_2838-0.jpg',
        help='Path to Image Directory',
    )

    parser.add_argument(
        '--image_type',
        type=str,
        default='jpg',
        help='Number of Images to Process',
    )

    parser.add_argument(
        '--is_halfsize',
        type=int,
        default=1,
        help='Using subfix "_halfsize" of image for low memory GPU => 0 (False), 1 (True)',
    )

    parser.add_argument(
        '--divider',
        type=int,
        default=1,
        help='Divide camera parameter',
    )

    args = parser.parse_args()

    main(**args.__dict__)
