import asyncio

from client_ssl import main

# REMINDER!! CHANGE CAMERA INTRINSIC

image_dir_name = "IMG_2838/2"  # Dataset, IMG_2838/2
image_path = f"images/TDPK/{image_dir_name}/540"
num_images = 0
image_type = "jpg"
is_half = False
divider = 1  # 1,2,4,8,16 divider of camera param

poses = None

asyncio.run(main(num_images, image_path, image_type, is_half, divider))
