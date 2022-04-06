import cv2
import glob
import os
import ShapeDetector
import Label_processing_Unity
import time
import API_GET
import Utils
import sys
import argparse
from PIL import Image

#####################################################################################################################
#####################################################################################################################

def main(img, customerAndSite):
    # Split the customer name and the site name
    customer, site = Utils.CustomerAndSiteSpliter(customerAndSite)

    # Read RegexFile to have all infos
    pathToRegexFile = '{}\\TestRegex.json'.format(os.path.dirname(__file__))
    regexp, room, type, background = Utils.ReadRegex(pathToRegexFile, customer, site)

    # Read API URL and Headers from conf file
    pathToEnvFile = "{}\\.env.json".format(os.path.dirname(__file__))
    url, token, headers = Utils.GetUrlAndToken(pathToEnvFile)

    # Return the sites that are available for the tenant provided
    current = time.time()
    siteAvailable = API_GET.GetSitesNames(customer, url, headers)
    if site not in siteAvailable:
        print("\nThe site name is wrong or there are no available sites for this customer in the Database")
        sys.exit()
    print("\nPerformed API GET in: {} s".format(time.time() - current))

    Label_processing_Unity.OCRAndCorrection(img, site, regexp, type, background)

#####################################################################################################################
#####################################################################################################################

def moulinette(customerAndSite):
    print("Début de la moulinette")
    path1 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Hololens\\*.jpg"
    path2 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\PCY\\*.jpg"
    path3 = "C:\\Users\\vince\\Nextcloud\\Ogree\\3_Unity\\3.2_AR\\Photos_visite_DC\\Avril\\NOE\\*.jpg"

    # Split the customer name and the site name
    customer, site = Utils.CustomerAndSiteSpliter(customerAndSite)

    # Read RegexFile to have all infos
    pathToRegexFile = '{}\\TestRegex.json'.format(os.path.dirname(__file__))
    regexp, room, type, background = Utils.ReadRegex(pathToRegexFile, customer, site)

    # Read API URL and Headers from conf file
    pathToEnvFile = "{}\\.env.json".format(os.path.dirname(__file__))
    url, token, headers = Utils.GetUrlAndToken(pathToEnvFile)

    # Return the sites that are available for the tenant provided
    current = time.time()
    siteAvailable = API_GET.GetSitesNames(customer, url, headers)
    if site not in siteAvailable:
        print("\nThe site name is wrong or there are no available sites for this customer in the Database")
        sys.exit()
    print("\nPerformed API GET in: {} s".format(time.time() - current))

    for img in glob.glob(path3):
        #Reading the image with opencv
        cv_img = cv2.imread(img)
        Label_processing_Unity.OCRAndCorrection(cv_img, site, regexp, type, background)
    return

#####################################################################################################################
#####################################################################################################################

if __name__ == "__main__":
    # COMMAND OPTIONS
    parser = argparse.ArgumentParser(description='Perform OCR from data (image + tenant) sent from Hololens')
    parser.add_argument('--image',
                        help="""Specify the path to an image to make OCR""",
                        required=True)

    parser.add_argument('--site',
                        help="""Specify the the tenant""",
                        required=True)

    # Parse Args START //////////
    args = vars(parser.parse_args())
    if ('image' not in args or args['image'] == None):
        print('Please specify the path to an image to make OCR')
        sys.exit()
    else:
        if args['image'].lower().endswith(('.png', '.jpg', '.jpeg', '.tiff', '.bmp', '.gif')):
            testValidity = Image.open(args['image'])
            try:
                testValidity.verify()
                img = cv2.imread(args['image'])
            except Exception:
                print('Invalid image, please verify the content')
                sys.exit()
        else:
            print('\nThe format of the image is not correct.\nIt should be in the following list: .png, .jpg, .jpeg, .tiff, .bmp, .gif')
            sys.exit()
    if ('site' not in args or args['site'] == None):
        print('Please specify a tenant to get available sites')
        sys.exit()
    else:
        tenantAndSite = args['site']
    # main(img, tenantAndSite)
    moulinette(tenantAndSite)


