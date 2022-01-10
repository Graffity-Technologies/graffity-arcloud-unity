import asyncio

from client_multiproc import main

print('INIT CACHE')

confs = [
    {
        "res": "1080",
        "divider": 2,
    },
]

poses = None

async def loc():
    for conf in confs:
        image_dir_name = "IMG_2838/4/" + conf["res"]
        image_path = f"images/TDPK/{image_dir_name}"
        num_images = 2
        image_type = "jpg"
        is_half = False
        divider = conf["divider"]

        main(num_images, image_path, image_type, is_half, divider)

asyncio.run(loc())
