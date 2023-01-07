# Copyright (c) 2018, ETH Zurich and UNC Chapel Hill.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
#
#     * Redistributions of source code must retain the above copyright
#       notice, this list of conditions and the following disclaimer.
#
#     * Redistributions in binary form must reproduce the above copyright
#       notice, this list of conditions and the following disclaimer in the
#       documentation and/or other materials provided with the distribution.
#
#     * Neither the name of ETH Zurich and UNC Chapel Hill nor the names of
#       its contributors may be used to endorse or promote products derived
#       from this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
# ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDERS OR CONTRIBUTORS BE
# LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
# CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
# SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
# CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
# POSSIBILITY OF SUCH DAMAGE.

import argparse
import numpy as np
import open3d

from read_write_model import read_model, write_model, qvec2rotmat, rotmat2qvec


class Model:
    def __init__(self):
        self.cameras = []
        self.images = []
        self.points3D = []
        self.__vis = None

    def read_model(self, path, ext=""):
        self.cameras, self.images, self.points3D = read_model(path, ext)

    def add_points(self, min_track_len=3, remove_statistical_outlier=True):
        pcd = open3d.geometry.PointCloud()

        xyz = []
        rgb = []
        for point3D in self.points3D.values():
            track_len = len(point3D.point2D_idxs)
            if track_len < min_track_len:
                continue
            xyz.append(point3D.xyz)
            rgb.append(point3D.rgb / 255)

        pcd.points = open3d.utility.Vector3dVector(xyz)
        pcd.colors = open3d.utility.Vector3dVector(rgb)

        # remove obvious outliers
        if remove_statistical_outlier:
            [pcd, _] = pcd.remove_statistical_outlier(nb_neighbors=20,
                                                      std_ratio=2.0)

        # open3d.visualization.draw_geometries([pcd])
        self.__vis.add_geometry(pcd)
        self.__vis.poll_events()
        self.__vis.update_renderer()

    def add_cameras(self, scale=1):
        frames = []
        for img in self.images.values():
            # rotation
            R = qvec2rotmat(img.qvec)

            # translation
            t = img.tvec

            # invert
            t = -R.T @ t
            R = R.T

            # intrinsics
            cam = self.cameras[img.camera_id]

            if cam.model in ("SIMPLE_PINHOLE", "SIMPLE_RADIAL", "RADIAL"):
                fx = fy = cam.params[0]
                cx = cam.params[1]
                cy = cam.params[2]
            elif cam.model in ("PINHOLE", "OPENCV", "OPENCV_FISHEYE"):
                fx = cam.params[0]
                fy = cam.params[1]
                cx = cam.params[2]
                cy = cam.params[3]
            else:
                raise Exception("Camera model not supported")

            # intrinsics
            K = np.identity(3)
            K[0, 0] = fx
            K[1, 1] = fy
            K[0, 2] = cx
            K[1, 2] = cy
            # print(f'fx: {fx}, fy: {fy}, cx: {cx}, cy: {cy}, width: {cam.width}, height: {cam.height}, scale: {scale}')
            # create axis, plane and pyramed geometries that will be drawn
            cam_model = draw_camera(K, R, t, cam.width, cam.height, scale)
            frames.extend(cam_model)

        # add geometries to visualizer
        for i in frames:
            self.__vis.add_geometry(i)

    def create_window(self):
        # open3d.visualization.webrtc_server.enable_webrtc()
        self.__vis = open3d.visualization.Visualizer()
        self.__vis.create_window()

    def show(self):
        self.__vis.poll_events()
        self.__vis.update_renderer()
        self.__vis.run()
        self.__vis.destroy_window()

    def process_ext_pose(self, qvec, tvec, fx, fy, cx, cy, width, height, scale):
        R = qvec2rotmat(qvec)
        t = tvec
        t = -R.T @ t
        R = R.T

        # intrinsics
        K = np.identity(3)
        K[0, 0] = fx
        K[1, 1] = fy
        K[0, 2] = cx
        K[1, 2] = cy
        cam_model = draw_camera(K, R, t, width, height,
                                scale, color=[0, 1.0, 0.0])
        for i in cam_model:
            self.__vis.add_geometry(i)


def draw_camera(K, R, t, w, h,
                scale=1, color=[0.8, 0.2, 0.8]):
    """Create axis, plane and pyramed geometries in Open3D format.
    :param K: calibration matrix (camera intrinsics)
    :param R: rotation matrix
    :param t: translation
    :param w: image width
    :param h: image height
    :param scale: camera model scale
    :param color: color of the image plane and pyramid lines
    :return: camera model geometries (axis, plane and pyramid)
    """

    # intrinsics
    K = K.copy() / scale
    Kinv = np.linalg.inv(K)

    # 4x4 transformation
    T = np.column_stack((R, t))
    T = np.vstack((T, (0, 0, 0, 1)))

    # axis
    axis = open3d.geometry.TriangleMesh.create_coordinate_frame(
        size=0.5 * scale)
    axis.transform(T)

    # points in pixel
    points_pixel = [
        [0, 0, 0],
        [0, 0, 1],
        [w, 0, 1],
        [0, h, 1],
        [w, h, 1],
    ]

    # pixel to camera coordinate system
    points = [Kinv @ p for p in points_pixel]

    # image plane
    width = abs(points[1][0]) + abs(points[3][0])
    height = abs(points[1][1]) + abs(points[3][1])
    plane = open3d.geometry.TriangleMesh.create_box(width, height, depth=1e-6)
    plane.paint_uniform_color(color)
    plane.translate([points[1][0], points[1][1], scale])
    plane.transform(T)

    # pyramid
    points_in_world = [(R @ p + t) for p in points]
    lines = [
        [0, 1],
        [0, 2],
        [0, 3],
        [0, 4],
    ]
    colors = [color for i in range(len(lines))]
    line_set = open3d.geometry.LineSet(
        points=open3d.utility.Vector3dVector(points_in_world),
        lines=open3d.utility.Vector2iVector(lines))
    line_set.colors = open3d.utility.Vector3dVector(colors)

    # return as list in Open3D format
    return [axis, plane, line_set]


def parse_args():
    parser = argparse.ArgumentParser(
        description="Visualize COLMAP binary and text models")
    parser.add_argument("--input_model", required=True,
                        help="path to input model folder")
    parser.add_argument("--input_format", choices=[".bin", ".txt"],
                        help="input model format", default="")
    args = parser.parse_args()
    return args


def main():
    args = parse_args()

    # res_qvec = np.array([float(t)
    #                      for t in input('enter qvec: ').strip().split()])
    # res_tvec = np.array([float(t)
    #                      for t in input('enter tvec: ').strip().split()])

    # 4281
    # q_dict = {
    #     "qw": 0.17629979255288242,
    #     "qx": 0.618798653548496,
    #     "qy": 0.6228191641375148,
    #     "qz": -0.44508751756865444
    # }
    # t_dict = {
    #     "tx": -33.27913582177968,
    #     "ty": 73.89801065768263,
    #     "tz": 95.61998827099922
    # }
    # TSC images/TSC_IMG_4914.png
    # q_dict = {
    #     "qw": -0.1201637135141186,
    #     "qx": 0.04262010629602899,
    #     "qy": 0.991506372490534,
    #     "qz": -0.025677262402273554
    # }
    # t_dict = {
    #     "tx": -0.4939350195674772,
    #     "ty": 0.2984812026372995,
    #     "tz": 5.065224013845427
    # }
    # TSC images/TSC_IMG_4915.png
    q_dict = {
        "qw": 0.2202093730525025,
        "qx": 0.10453592618489893,
        "qy": 0.9522990861156951,
        "qz": -0.18359336246090524
    }
    t_dict = {
        "tx": -24.432575518920114,
        "ty": 9.953610085477942,
        "tz": 50.647971053099134
    }
    # Unity horizon
    # q_dict = {
    #     "qw": 0.19947459180679766,
    #     "qx": 0.16782883306926488,
    #     "qy": -0.614401047447399,
    #     "qz": 0.7446843109060772
    # }
    # t_dict = {
    #     "tx": -3.524800729086278,
    #     "ty": 2.1055243534638515,
    #     "tz": 11.912750584504566
    # }
    # images/JustCo/IMG_4429.jpg
    # q_dict = {
    #     "qw": 0.6841639691050488,
    #     "qx": 0.721617291318632,
    #     "qy": -0.08757127640975618,
    #     "qz": 0.059324698030017824
    # }
    # t_dict = {
    #     "tx": -0.5741692848546434,
    #     "ty": 0.1928940234412658,
    #     "tz": -2.224292965510377
    # }
    res_qvec = np.array([q_dict["qw"], q_dict["qx"],
                        q_dict["qy"], q_dict["qz"]])
    res_tvec = np.array([t_dict["tx"], t_dict["ty"], t_dict["tz"]])
    print(res_qvec, res_tvec)

    # read COLMAP model
    model = Model()
    model.read_model(args.input_model, ext=args.input_format)

    print("num_cameras:", len(model.cameras))
    print("num_images:", len(model.images))
    print("num_points3D:", len(model.points3D))

    # display using Open3D visualization tools
    model.create_window()
    model.add_points()
    model.add_cameras(scale=0.05)

    scale = 1.0

    # fx: 1798.2926825734073, fy: 1798.2926825734073, cx: 960.0, cy: 540.0, width: 1920, height: 1080, scale: 0.25
    # fx: 3165.422982044238, fy: 3165.422982044238, cx: 1920.0, cy: 1080.0, width: 3840, height: 2160, scale: 0.25
    # fx, fy, cx, cy, width, height, scale = 1812, 1812, 960, 540, 1920, 1080, 1.00
    # fx, fy, cx, cy, width, height = 4608.000000, 4608.000000, 1920.000000, 1080.000000, 3840, 2160  # Dataset

    # Dataset Tops Club Rama 2 v1-60fps
    # fx, fy, cx, cy, width, height = 2304.000000, 2304.000000, 540.000000, 960.000000, 1080, 1920

    # Dataset Alibaba SG
    # fx, fy, cx, cy, width, height = 1152.000000, 1152.000000, 480.000000, 270.000000, 960, 540

    # Flap's Iphone 12 Unity
    # fx, fy, cx, cy, width, height = 473.183258, 473.183258, 321.801971, 238.419632, 640, 480

    # Bank's iPhone12 4k
    # fx, fy, cx, cy, width, height = 3000, 3000, 2000, 1500, 3024, 4032
    fx, fy, cx, cy, width, height = 750.0, 750.0, 500.0, 375.0, 756, 1008

    # Unity
    # fx, fy, cx, cy, width, height = 1533.91833, 1533.91833, 957.7629, 714.0067, 1920, 1440
    # fx, fy, cx, cy, width, height = 1533.91833, 1533.91833, 714.0067, 957.7629, 1440, 1920

    model.process_ext_pose(res_qvec, res_tvec, fx, fy,
                           cx, cy, width, height, scale)

    model.show()


if __name__ == "__main__":
    main()
