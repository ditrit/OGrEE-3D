import cv2
import numpy as np
import sys

img = cv2.imread('C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\PCY\\PCY1-R23.jpg')

def callback():
    pass

def test_color():
    cv2.namedWindow("trackBar")
    cv2.resizeWindow("trackBar", 600, 300)
    cv2.createTrackbar("hue_min", "trackBar", 15, 179, callback)
    cv2.createTrackbar("hue_max", "trackBar", 15, 179, callback)
    cv2.createTrackbar("sat_min", "trackBar", 149, 255, callback)
    cv2.createTrackbar("sat_max", "trackBar", 255, 255, callback)
    cv2.createTrackbar("val_min", "trackBar", 141, 255, callback)
    cv2.createTrackbar("val_max", "trackBar", 255, 255, callback)

    while True:
        # ret, image = video.read()
        img = cv2.imread(
            'C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\PCY\\PCY1-R23.jpg')
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HLS)

        hue_min = cv2.getTrackbarPos("hue_min", "trackBar")
        hue_max = cv2.getTrackbarPos("hue_max", "trackBar")
        sat_min = cv2.getTrackbarPos("sat_min", "trackBar")
        sat_max = cv2.getTrackbarPos("sat_max", "trackBar")
        val_min = cv2.getTrackbarPos("val_min", "trackBar")
        val_max = cv2.getTrackbarPos("val_max", "trackBar")

        lower = np.array([hue_min, sat_min, val_min])
        upper = np.array([hue_max, sat_max, val_max])

        mask = cv2.inRange(hsv, lower, upper)
        contours, hei = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        x_min, y_min, x_max, y_max = 5000, 5000, 0, 0
        for c in contours:
            area = cv2.contourArea(c)
            if area > 500:
                x, y, w, h = cv2.boundingRect(c)
                if x < x_min:
                    x_min = x
                if y < y_min:
                    y_min = y
                if x + w > x_max:
                    x_max = x + w
                if y + h > y_max:
                    y_max = y + h
                cv2.rectangle(img, (x, y), (x+w, y+h), (0, 255, 0), 2)
                # print("x_min = {} y_min = {} x_max = {} y_max = {}".format(x_min, y_min, x_max, y_max))
        cropped_image = img[y_min:y_max, x_min:x_max]

        # if cropped_image.any():
        #     cv2.namedWindow("cropped", cv2.WINDOW_NORMAL)
        #     cv2.imshow("cropped", cropped_image)
        #     cv2.waitKey()
        cv2.namedWindow("original", cv2.WINDOW_NORMAL)
        cv2.imshow("original", img)
        cv2.waitKey(1)
    return

def ShapeAndColorDetector(img, color):

    #Convert to hsv
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    #Set the range for the color to keep
    if color == 'orange':
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        hue_min, sat_min, val_min = 10, 149, 141
        hue_max, sat_max, val_max = 20, 255, 255
        lower = np.array([hue_min, sat_min, val_min])
        upper = np.array([hue_max, sat_max, val_max])

    elif color == 'white':
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HLS)
        hue_min, lumin_min, sat_min = 0, 240, 0
        hue_max, lumin_max, sat_max = 179, 255, 255
        lower = np.array([hue_min, lumin_min, sat_min])
        upper = np.array([hue_max, lumin_max, sat_max])

    elif color == 'yellow':
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        hue_min, sat_min, val_min = 24, 100, 141
        hue_max, sat_max, val_max = 30, 255, 255
        lower = np.array([hue_min, sat_min, val_min])
        upper = np.array([hue_max, sat_max, val_max])

    elif color == 'red':
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        hue_min, sat_min, val_min = 0, 149, 141
        hue_max, sat_max, val_max = 10, 255, 255
        lower = np.array([hue_min, sat_min, val_min])
        upper = np.array([hue_max, sat_max, val_max])
    else:
        print("The color provided is not listed in the accepted colors, please provide one of the following colors: 'orange', 'white, 'yellow', 'red'")
        sys.exit()

    #Create the mask to determine the contour
    mask = cv2.inRange(hsv, lower, upper)
    contours, hei = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    #Initialize the bounds of our cropped image
    x_min, y_min, x_max, y_max = 5000, 5000, 0, 0

    #Loop over all contours, add rectangle near detected area, set the bounds
    for c in contours:
        area = cv2.contourArea(c)
        if area > 400:
            x, y, w, h = cv2.boundingRect(c)
            if x < x_min:
                x_min = x
            if y < y_min:
                y_min = y
            if x + w > x_max:
                x_max = x + w
            if y + h > y_max:
                y_max = y + h
            cv2.rectangle(img, (x, y), (x+w, y+h), (0, 0, 255), 2)

    #Create cropped image
    cropped_image = img[y_min:y_max, x_min:x_max]
    # print("x_min = {} y_min = {} x_max = {} y_max = {}".format(x_min, y_min, x_max, y_max))

    if cropped_image.any():
        # cv2.namedWindow("cropped", cv2.WINDOW_NORMAL)
        # cv2.imshow("cropped", cropped_image)
        # cv2.waitKey()
        return cropped_image
    else:
        return img


if __name__ == "__main__":
    # ShapeAndColorDetector(img, 'orange')
    test_color()