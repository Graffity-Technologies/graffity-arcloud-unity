import argparse
import atexit
import logging
import multiprocessing
import operator
import sys
import glob
import time
from argparse import ArgumentParser

import grpc
import cv2
import protos.image.image_pb2 as image_pb
import protos.image.image_pb2_grpc as image_pb_grpc
from google.protobuf.json_format import MessageToDict

_PROCESS_COUNT = 2
_MAXIMUM_CANDIDATE = 10000

# Each worker process initializes a single channel after forking.
# It's regrettable, but to ensure that each subprocess only has to instantiate
# a single channel to be reused across all RPCs, we use globals.
_worker_channel_singleton = None
_worker_stub_singleton = None

_LOGGER = logging.getLogger(__name__)

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

    # aachen
    # cameraInfo = image_pb.CameraInfo(
    #     pixelFocalLength=1469.2,
    #     principalPointX=800,
    #     principalPointY=600,
    #     radialDistortion=-0.0353019
    # )

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

    # print("cameraInfo", cameraInfo)

    if num_images != 0:
        query_images = query_images[0:num_images]

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

    for idx, img in enumerate(query_images):
        # print(idx, img)
        im = cv2.imread(img)
        is_success, im_buf_arr = cv2.imencode(".jpg", im)
        byte_im = im_buf_arr.tobytes()

        msg = image_pb.ImageRequest(
            message=img, # os.path.basename(img)
            bytesImage=byte_im,
            cameraInfo=cameraInfo,
            gpsPosition=gpsPosition
        )

        # print("Client send: %s" % msg.message)
        yield msg


def _shutdown_worker():
    _LOGGER.info('Shutting worker process down.')
    if _worker_channel_singleton is not None:
        _worker_channel_singleton.stop()


def _initialize_worker(server_address):
    global _worker_channel_singleton  # pylint: disable=global-statement
    global _worker_stub_singleton  # pylint: disable=global-statement
    _LOGGER.info('Initializing worker process.')
    _worker_channel_singleton = grpc.insecure_channel(server_address)
    _worker_stub_singleton = image_pb_grpc.ImageStub(
        _worker_channel_singleton)
    atexit.register(_shutdown_worker)


def _run_worker_query(primality_candidate):
    _LOGGER.info('Checking primality of %s.', primality_candidate)
    return _worker_stub_singleton.check(
        image_pb2.PrimeCandidate(candidate=primality_candidate))


def _calculate_primes(server_address):
    worker_pool = multiprocessing.Pool(processes=_PROCESS_COUNT,
                                       initializer=_initialize_worker,
                                       initargs=(server_address,))
    check_range = range(2, _MAXIMUM_CANDIDATE)
    primality = worker_pool.map(_run_worker_query, check_range)
    primes = zip(check_range, map(operator.attrgetter('isPrime'), primality))
    return tuple(primes)


def main():
    msg = 'Determine the primality of the first {} integers.'.format(
        _MAXIMUM_CANDIDATE)
    parser = argparse.ArgumentParser(description=msg)
    parser.add_argument('server_address',
                        help='The address of the server (e.g. localhost:50051)')
    args = parser.parse_args()
    primes = _calculate_primes(args.server_address)
    print(primes)


if __name__ == '__main__':
    handler = logging.StreamHandler(sys.stdout)
    formatter = logging.Formatter('[PID %(process)d] %(message)s')
    handler.setFormatter(formatter)
    _LOGGER.addHandler(handler)
    _LOGGER.setLevel(logging.INFO)
    main()