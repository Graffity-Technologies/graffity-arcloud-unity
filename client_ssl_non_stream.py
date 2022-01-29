from __future__ import print_function
import time
import cv2
import grpc
import glob
import os

import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc

from google.protobuf.json_format import MessageToDict
from argparse import ArgumentParser

host = 'vps.graffity.services'


def generate_messages(
    num_images: int,
    image_path: str,
    image_type: str,
    is_halfsize: bool,
    divider: int,
):
    query_images = []

    for image in glob.iglob(
        f'{image_path}/' + '**/*.' + image_type, recursive=True
    ):
        if (is_halfsize):
            if "halfsize" in image:
                query_images.append(image)
            else:
                continue
        else:
            if "halfsize" not in image:
                query_images.append(image)

    # iPhone12 .builtInDualWideCamera
    cameraInfo = image_pb.CameraInfo(
        pixelFocalLength=3000/2 if is_halfsize else 3000/divider,
        principalPointX=2000/2 if is_halfsize else 2000/divider,
        principalPointY=1500/2 if is_halfsize else 1500/divider,
        radialDistortion=0/divider
    )

    # TDPK Dataset - Horizontal
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=4608.000000/divider,
    #     principalPointX=1920.000000/divider,
    #     principalPointY=1080.000000/divider,
    #     radialDistortion=0.000000/divider
    # )

    # # TDPK Dataset - Vertical
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=4608.000000,
    #     principalPointX=1920.000000,
    #     principalPointY=1080.000000,
    #     radialDistortion=0.000000
    # )

    if num_images != 0:
        query_images = query_images[0:1]

    # inside TDPK
    gpsPosition = image_pb.Position(
        latitude=13.685685,
        longitude=100.611000,
        altitude=0.0
    )
    # inside OneSiam Skywalk
    # gpsPosition = image_pb.Position(
    #     latitude=13.746103,
    #     longitude=100.530739,
    #     altitude=0.0
    # )
    img = query_images[0]
    im = cv2.imread(img)
    is_success, im_buf_arr = cv2.imencode(".jpg", im)
    byte_im = im_buf_arr.tobytes()

    msg = image_pb.ImageRequest(
        message='request-image-' + os.path.basename(img),
        bytesImage=byte_im,
        cameraInfo=cameraInfo,
        gpsPosition=gpsPosition
    )

    print("Client send: %s" % msg.message)
    return msg


def main(
    num_images: int,
    image_path: str,
    image_type: str,
    is_halfsize: bool,
    divider: int,
):
    res = []
    start_time_total = time.time()

    credentials = grpc.ssl_channel_credentials(
        root_certificates=None, private_key=None, certificate_chain=None)

    with grpc.secure_channel(host, credentials) as channel:
        stub = image_pb_grpc.ImageStub(channel)

        metadata = [
            ('x-graff-api-key', 'KpN4I5l4G8gFB8HVx6Xd')
        ]

        response = stub.SendImage(
            generate_messages(
                num_images,
                image_path,
                image_type,
                is_halfsize=False if is_halfsize == 0 else True,
                divider=divider,
            ),
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
        default=0,
        help='Number of Images to Process',
    )

    parser.add_argument(
        '--image_path',
        type=str,
        default='server/tests/query_images/true_digital_park',
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
