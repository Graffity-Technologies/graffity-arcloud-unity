from client_ssl_non_stream import main

confs = [
    {
        "res": "1080",
        "divider": 2,
    },
    # {
    #     "res": "540",
    #     "divider": 4,
    # },
]

poses = None


def loc():
    for conf in confs:
        image_dir_name = conf["res"]  # "IMG_2838/2/" +
        image_path = f"images/BankRoom/{image_dir_name}"
        num_images = 1
        image_type = "jpg"
        is_half = False
        divider = conf["divider"]

        main(num_images, image_path, image_type, is_half, divider)


if __name__ == '__main__':
    loc()
