import asyncio

from client import main

# REMINDER!! CHANGE CAMERA INTRINSIC

image_dir_name = "IMG_2838/3"  # Dataset, IMG_2838/2
image_path = f"images/TDPK/{image_dir_name}/1080"
num_images = 5
image_type = "jpg"
is_half = False
divider = 1  # 1,2,4,8,16 divider of camera param

poses = None

asyncio.run(main(num_images, image_path, image_type, is_half, divider))
