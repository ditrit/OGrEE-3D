import cv2
import numpy as np

# read image
img = cv2.imread('Images/image.jpg')

# define kernel size
kernel = np.ones((7, 7), np.uint8)

colourToFind = np.uint8([[[26, 130, 231]]])
hsvColourToFind = cv2.cvtColor(colourToFind, cv2.COLOR_BGR2HSV)
lowerBound = hsvColourToFind[0][0][0] - 10
upperBound = hsvColourToFind[0][0][0] + 10

# lower bound and upper bound for Orange color
ORANGE_MIN = np.array([lowerBound, 50, 50], np.uint8)
ORANGE_MAX = np.array([upperBound, 255, 255], np.uint8)
print(ORANGE_MIN)
print(ORANGE_MAX)

def SegmentOrangeLabel(img):
    # convert to hsv colorspace
    hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

    # find the colors within the boundaries
    mask = cv2.inRange(hsv, ORANGE_MIN, ORANGE_MAX)

    # Remove unnecessary noise from mask
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)

    # Segment only the detected region
    segmented_img = cv2.bitwise_and(img, img, mask=mask)

    # Find contours from the mask
    contours, hierarchy = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    # Draw contour on original image
    output = cv2.drawContours(img, contours, -1, (0, 0, 255), 3)

    # Draw contour on segmented image
    # output = cv2.drawContours(segmented_img, contours, -1, (0, 0, 255), 3)

    # Showing the output
    cv2.namedWindow("Output", cv2.WINDOW_NORMAL)
    cv2.resizeWindow('Output', 1000, 1000)
    cv2.imshow("Output", output)

    cv2.waitKey(0)
    cv2.destroyAllWindows()
    return

def main():
    SegmentOrangeLabel(img)
    return

if __name__ == '__main__':
    main()






















