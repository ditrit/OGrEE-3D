import OCR
import argparse
import cv2
import API_GET
from PIL import Image
import sys
from os.path import exists
import os.path



#Read API URL and Headers from conf file
pathToConfFile = "{}\\conf.json".format(os.path.dirname(__file__))
file_exists = exists(pathToConfFile)
if file_exists:
    f = open(pathToConfFile, "r")

    url = str(f.readline()[:-1])
    token = str(f.readline())
    headers = {
        'Authorization': token
    }
    f.close()
else:
    print("\nCannot find configuration file")
    sys.exit()

def main():
    img, tenantName = parsing()
    pathToRegexfile = '{}\\regex{}.json'.format(os.path.dirname(__file__), tenantName)
    file_exists = exists(pathToRegexfile)
    if not file_exists:
        print("\nCannot find regex file")
        return
    siteAvailable = API_GET.GetSitesNames(tenantName, url, headers)
    if not siteAvailable:
        print("\nThe tenant name is wrong or there are no available sites for this tenant")
        return
    results = OCR.PerformOCR(img, 'easyocr')
    for (bbox, text, prob) in results:
        text = OCR.ReplaceSymbol(text)
        site, room, rack, output = OCR.RecoverSiteRoomRack(text, bbox, img, pathToRegexfile, siteAvailable)
        if site is not None and room is not None and rack is not None:
            OCR.DisplayImage(output)
            return
    print("\nCould not find rack label on the picture, please try again\n")
    OCR.DisplayImage(img)
    return

def parsing():
    # COMMAND OPTIONS
    parser = argparse.ArgumentParser(description='Perform OCR from data (image + tenant) sent from Hololens')
    parser.add_argument('-i',
                        help="""Specify the path to an image to make OCR""",
                        required=True)

    parser.add_argument('-t',
                        help="""Specify the the tenant""",
                        required=True)

    # Parse Args START //////////
    args = vars(parser.parse_args())
    if ('i' not in args or args['i'] == None):
        print('Please specify the path to an image to make OCR')
    else:
        if args['i'].lower().endswith(('.png', '.jpg', '.jpeg', '.tiff', '.bmp', '.gif')):
            testValidity = Image.open(args['i'])
            try:
                testValidity.verify()
                img = cv2.imread(args['i'])
            except Exception:
                print('Invalid image, please verify the content')
        else:
            print(
                '\nThe format of the image is not correct.\nIt should be in the following list: .png, .jpg, .jpeg, .tiff, .bmp, .gif')
            sys.exit()
    if ('t' not in args or args['t'] == None):
        print('Please specify a tenant to get available sites')
    else:
        tenantName = args['t']
    # Parse Args END //////////
    return img, tenantName

if __name__ == '__main__':
    main()




