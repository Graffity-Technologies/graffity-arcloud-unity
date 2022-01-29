import time
import grpc
import logging
import sys
import os
import atexit
import multiprocessing
import glob

import cv2
import image_pb2 as image_pb
import image_pb2_grpc as image_pb_grpc

from argparse import ArgumentParser

_LOGGER = logging.getLogger(__name__)
_HOST = "localhost"
_PORT = "50054"
_worker_channel_singleton = None
_worker_stub_singleton = None
_PROCESS_COUNT = int(os.getenv("NUM_PROCESS", multiprocessing.cpu_count() / 4))

print('image_pb', image_pb.ImageRequest())
print('image_pb_grpc', image_pb_grpc)


def generate_messages(
    image_requests,
):
    for idx, image in enumerate(image_requests):
        # print(idx, img)

        msg = image_pb.ImageRequest(
            message=image.message,
            bytesImage=image.bytesImage,
            cameraInfo=image.cameraInfo,
            gpsPosition=image.gpsPosition,
        )

        # print("Client send: %s" % msg.message)
        yield msg


def _shutdown_worker():
    print("Shutting worker process down.")
    if _worker_channel_singleton is not None:
        _worker_channel_singleton.stop()


def _initialize_worker(host):
    global _worker_channel_singleton
    global _worker_stub_singleton
    print("Initializing worker process.")

    _worker_channel_singleton = grpc.insecure_channel(host)
    _worker_stub_singleton = image_pb_grpc.ImageStub(_worker_channel_singleton)
    atexit.register(_shutdown_worker)


def _run_worker_query(
    num_images: int,
    image_path: str,
    image_type: str,
    is_halfsize: bool,
    divider: int,
):
    print("Checking %s.", image_path)

    metadata = [("x-graff-api-key", "KpN4I5l4G8gFB8HVx6Xd")]

    print("_worker_stub_singleton in _run_worker_query", _worker_stub_singleton)

    return _worker_stub_singleton.SendImage(
        generate_messages(
            num_images,
            image_path,
            image_type,
            is_halfsize=False if is_halfsize == 0 else True,
            divider=divider,
        ),
        metadata=metadata,
    )


def client(
    image_requests,  # list of "image_pb.ImageRequest" object
):
    results = []
    start_time_total = time.time()

    worker_pool = multiprocessing.Pool(
        processes=_PROCESS_COUNT,
        initializer=_initialize_worker,
        initargs=(f"{_HOST}:{_PORT}",),
    )

    print("worker_pool", worker_pool)
    print('image_requests len', len(image_requests))

    responses = worker_pool.map(_run_worker_query, image_requests)

    for response in responses:
        print(response.message)
        # print("Client received worldCoor: ", response.worldCoor)
        # print("Client received colmapCoor: ", response.colmapCoor)
        results.append(response)

    print("   ")
    print("Total Responses Time:", (time.time() - start_time_total), "seconds")

    return results


def main(
    num_images: int,
    image_path: str,
    image_type: str,
    is_halfsize: bool,
    divider: int,
):
    image_requests = []
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

    cameraInfo = image_pb.CameraInfo(
        pixelFocalLength=3000/2 if is_halfsize else 3000/divider,
        principalPointX=2000/2 if is_halfsize else 2000/divider,
        principalPointY=1500/2 if is_halfsize else 1500/divider,
        radialDistortion=0/divider
    )

    if num_images != 0:
        query_images = query_images[0:num_images]

    # inside TDPK
    gpsPosition = image_pb.Position(
        latitude=13.685685,
        longitude=100.611000,
        altitude=0.0
    )

    for idx, img in enumerate(query_images):
        print(idx, img)
        im = cv2.imread(img)
        is_success, im_buf_arr = cv2.imencode(".jpg", im)
        byte_im = im_buf_arr.tobytes()

        msg = image_pb.ImageRequest(
            message='request-image-' + os.path.basename(img),
            bytesImage=byte_im,
            cameraInfo=cameraInfo,
            gpsPosition=gpsPosition
        )

        image_requests.append(msg)

        print("append image: %s" % msg.message)

    client(image_requests=image_requests)


if __name__ == "__main__":
    handler = logging.StreamHandler(sys.stdout)
    formatter = logging.Formatter("[PID %(process)d] %(message)s")
    handler.setFormatter(formatter)
    _LOGGER.addHandler(handler)
    _LOGGER.setLevel(logging.INFO)

    parser = ArgumentParser()

    args = parser.parse_args()

    main(**args.__dict__)
