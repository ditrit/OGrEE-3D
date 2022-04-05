import cv2
import glob
import os
import ShapeDetector
import Label_processing_Unity
import time
import API_GET
import Utils
import sys

def OCRAndCorrection(img, tenantName, siteAvailable):
    print("\n\n\nBeginning the processing of the image...")
    start = time.time()
    current = start

    #Cropping the image
    result = ShapeDetector.ShapeAndColorDetector(img, 'orange')
    print("\nCropped image in: {} s".format(time.time() - current))

    # Perform OCR + post-processing on the cropped_image to recover the name of the site, room and rack
    site, room, rack = Label_processing_Unity.main(result, tenantName, siteAvailable, True)
    # return label if it was found
    if site is not None and room is not None and rack is not None:
        json = site + room + '-' + rack
        print("\nTotal time: {} s".format(time.time() - start))
        print("\nThe label read is: {}".format(json))
        return
    else:
        print("\nCould not find rack label on cropped image. Trying on the full image.")
        site, room, rack = Label_processing_Unity.main(img, tenantName, siteAvailable, False)
        if site is not None and room is not None and rack is not None:
            json = site + room + '-' + rack
            print("\nTotal time: {} s".format(time.time() - start))
            print("\nThe label read is: {}".format(json))
            return
        if site is None or room is None or rack is None:
            json = "\nCould not find rack label on the picture, please try again"
            print("\nTotal time: {} s".format(time.time() - start))
            print("\nThe label read is: {}".format(json))
            return

def main():
    #Read API URL and Headers from conf file
    pathToConfFile = "{}\\conf.json".format(os.path.dirname(__file__))
    url, token, headers = Utils.GetUrlAndToken(pathToConfFile)

    tenantName = 'EDF'
    path = os.path.dirname(__file__)
    path1 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Hololens\\*.jpg"
    path2 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\PCY\\*.jpg"
    path3 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\NOE\\*.jpg"

    current = time.time()

    # Return the sites that are available for the tenant provided
    siteAvailable = API_GET.GetSitesNames(tenantName, url, headers)
    if not siteAvailable:
        print("\nThe tenant name is wrong or there are no available sites for this tenant")
        sys.exit()
    print("\nPerformed API GET in: {} s".format(time.time() - current))

    for img in glob.glob(path2):
        #Reading the image with opencv
        cv_img = cv2.imread(img)
        OCRAndCorrection(cv_img, tenantName, siteAvailable)

if __name__ == "__main__":
    main()


