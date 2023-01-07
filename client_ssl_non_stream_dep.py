from __future__ import print_function
import time
import cv2
import grpc
import glob
import os

import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc
# import protos.vpsimage.vpsimage_pb2 as vpsimage_pb
# import protos.vpsimage.vpsimage_pb2_grpc as vpsimage_pb_grpc

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
        query_images.append(image)
        # if (is_halfsize):
        #     if "halfsize" in image:
        #         query_images.append(image)
        #     else:
        #         continue
        # else:
        #     if "halfsize" not in image:
        #         query_images.append(image)

    # Flap's iPhone12 Unity
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=473.183258 if is_halfsize else 3000/divider,
    #     principalPointX=321.801971 if is_halfsize else 2000/divider,
    #     principalPointY=238.419632 if is_halfsize else 1500/divider,
    #     radialDistortion=0/divider
    # )

    # iPhone12 .builtInDualWideCamera
    cameraInfo = image_pb.CameraInfo(
        pixelFocalLength=3000/divider,
        principalPointX=2000/divider,
        principalPointY=1500/divider,
        radialDistortion=0/divider
    )

    # Unity Test
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=1533.91833,
    #     principalPointX=957.7629,
    #     principalPointY=714.0067,
    #     # principalPointX=714.0067,
    #     # principalPointY=957.7629,
    #     radialDistortion=0
    # )

    # Dataset - Horizontal
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=4608.000000/divider,
    #     principalPointX=1920.000000/divider,
    #     principalPointY=1080.000000/divider,
    #     radialDistortion=0.000000/divider
    # )

    # Dataset - Vertical
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=4608.000000,
    #     principalPointX=1080.000000,
    #     principalPointY=1920.000000,
    #     radialDistortion=0.000000
    # )

    # Dataset Tops Club Rama 2 v1-60fps (W-1080,H-1920)
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=2304.000000,
    #     principalPointX=540.000000,
    #     principalPointY=960.000000,
    #     radialDistortion=0.000000
    # )

    # Dataset Alibaba Singapore
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=1152.000000,
    #     principalPointX=480.000000,
    #     principalPointY=270.000000,
    #     radialDistortion=0.000000
    # )

    print(cameraInfo)

    if num_images != 0:
        query_images = query_images[0:num_images]

    # inside TDPK
    # gpsPosition = image_pb.Position(
    #     latitude=13.685685,
    #     longitude=100.611000,
    #     altitude=0.0
    # )
    # Flap Room
    # gpsPosition = image_pb.Position(
    #     latitude=13.605449,
    #     longitude=100.639991,
    #     altitude=0.0
    # )
    # Bank Room
    # gpsPosition = image_pb.Position(
    #     latitude=13.579003,
    #     longitude=100.109626,
    #     altitude=0.0
    # )
    # inside OneSiam Skywalk
    # gpsPosition = image_pb.Position(
    #     latitude=13.746103,
    #     longitude=100.530739,
    #     altitude=0.0
    # )
    # inside Silom Edge
    gpsPosition = image_pb.Position(
        latitude=13.729508407345941,
        longitude=100.53591335616113,
        altitude=0.0
    )
    # inside QSNCC
    # gpsPosition = image_pb.Position(
    #     latitude=13.724525383113425,
    #     longitude=100.55904048093257,
    #     altitude=0.0
    # )
    # Cyberrex Office
    # gpsPosition = image_pb.Position(
    #     latitude=13.688635327050685,
    #     longitude=100.62911668707221,
    #     altitude=0.0
    # )
    # Tops Club Rama 2
    # gpsPosition = image_pb.Position(
    #     latitude=13.665226831773348,
    #     longitude=100.43660298445566,
    #     altitude=0.0
    # )
    # Alibaba SG
    # gpsPosition = image_pb.Position(
    #     latitude=1.2978958785356183,
    #     longitude=103.85017626563776,
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
            ('x-graff-api-key', 'sk.LVA0eDNKc2ZTaTlQUmRSbmVEV0FycG5SRzFIMDZkbUxKTmZpYlZTcklLZ0V0eW1kdmVEYVZGWnBhM3VaOWpVdW9Za2E2dVcyS2VqVnBfMlJmTTNsRUVKbnVDRGd0cktY')
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
        print("Client received accuracy: ", response.accuracy)
        # print("Client received worldCoor: ", response.worldCoor)
        print("Client received colmapCoor: ", response.colmapCoor)
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
        default='images/JustCo',
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
        default=4,
        help='Divide camera parameter',
    )

    args = parser.parse_args()

    main(**args.__dict__)
