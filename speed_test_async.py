import asyncio
from client_async import main

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
        num_images = 1
        image_type = "jpg"
        is_half = False
        divider = conf["divider"]

        await main(num_images, image_path, image_type, is_half, divider)


if __name__ == '__main__':

    asyncio.run(loc())
