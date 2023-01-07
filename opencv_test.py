import cv2

cap = cv2.VideoCapture("IMG_4355_TRIM.mov")
count = 0
while cap.isOpened():
      ret, frame = cap.read()

      if ret:
        print(ret)
        count += 30
        cap.set(1, count)
      else:
        cap.release()
        break