# from pickle import load as PickleLoad
# import pickle

# db_images = PickleLoad(open("pkl_files/bank_room_points3D.pkl", "rb"))

# # print(db_images)
# output = None

# pickle.dump(db_images, output, -1)

# print(output)

import random
import pickle
# import utils
import collections

pkl_file = open('pkl_files/bank_room_points3D.pkl', 'rb')

CameraModel = collections.namedtuple(
    "CameraModel", ["model_id", "model_name", "num_params"])
Camera = collections.namedtuple(
    "Camera", ["id", "model", "width", "height", "params"])
BaseImage = collections.namedtuple(
    "Image", ["id", "qvec", "tvec", "camera_id", "name", "xys", "point3D_ids"])
Point3D = collections.namedtuple(
    "Point3D", ["id", "xyz", "rgb", "error", "image_ids", "point2D_idxs"])


def sample_from_dict(d, sample=1):
    keys = random.sample(list(d), sample)
    values = [d[k] for k in keys]
    return dict(zip(keys, values))


data1 = pickle.load(pkl_file)
print(len(data1))
print(type(data1))
print(sample_from_dict(data1))

pkl_file.close()
