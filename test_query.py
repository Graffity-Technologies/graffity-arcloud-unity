import argparse
import numpy as np

import visualize_model
import client_ssl as vps_client


def main():
    parser = argparse.ArgumentParser(
        description="Test & Visualize Query Image")
    parser.add_argument("--img_dir", required=True, type=str,
                        help="path to query image")
    parser.add_argument("--intrinsic", required=True, type=str,
                        help="camera intrinsic parameters fx fy cx cy")
    args = parser.parse_args()

    pose = vps_client.main(1, "", "", False, 1)
