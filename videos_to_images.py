import os
import glob
import re
import cv2
import logging

from pathlib import Path
from shutil import copy
from argparse import ArgumentParser
from tqdm import tqdm

logging.basicConfig(level=logging.NOTSET)


def main(video_path: Path, skip_frame: int, image_path: Path, video_type: str):
    logging.info(
        f'Performing Split videos from {video_path} to images with {image_path} path')
    try:
        if not os.path.exists(image_path):
            os.makedirs(image_path)

    except OSError:
        logging.error('Error: Creating directory of images')

    for filename in tqdm(
        glob.iglob(f'{video_path}/' + '**/*.' + video_type, recursive=True)
    ):
        logging.info(
            f'Working on {filename}')

        cap = cv2.VideoCapture(filename)

        cap_name = filename.split('/')[-1].split('.')[0]
        count = 0

        while cap.isOpened():
            ret, frame = cap.read()

            if ret:
                cv2.imwrite(f'{image_path}/{cap_name}' +
                            '-{:d}.jpg'.format(count), frame)
                count += skip_frame
                cap.set(1, count)
            else:
                cap.release()
                break


def split_images(images_dir, skip, img_type='jpg'):
    def numericalSort(value):
        numbers = re.compile(r'(\d+)')
        parts = numbers.split(value)
        parts[1::2] = map(int, parts[1::2])
        return parts

    c = 0
    images_des = images_dir + '/split_output'
    os.makedirs(images_des)

    for infile in tqdm(sorted(glob.glob(f'{images_dir}/' + '**/*.' + img_type, recursive=True), key=numericalSort)):
        if (c % skip == 0):
            # os.remove(infile)
            copy(infile, images_des, follow_symlinks=True)
        c += 1


if __name__ == '__main__':
    parser = ArgumentParser(description='Process video at given folder.')
    parser.add_argument(
        '--video_path',
        type=Path,
        required=True,
        help='Path to videos',
    )

    parser.add_argument(
        '--skip_frame',
        default=10,
        type=int,
        required=True,
        help='Number of skipping frame',
    )

    parser.add_argument(
        '--image_path',
        type=Path,
        required=True,
        help='Output path',
    )

    parser.add_argument(
        '--video_type',
        type=str,
        required=True,
        help='Case sentitive video type "mp4", "MOV", ...',
    )

    args = parser.parse_args()
    main(**args.__dict__)
