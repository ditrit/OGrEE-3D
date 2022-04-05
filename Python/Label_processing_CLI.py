import OCR
import argparse
import cv2
import API_GET
from PIL import Image
import sys
from os.path import exists
import os.path
import time


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
    # Flag for detecting if the next text is supposed to be the rack
    isNextTextRack = False
    isLabelSeparated = True
    site, room, rack = None, None, None

    #Parsing
    img, tenantName = parsing()

    pathToRegexfileSiteRoomOnly = '{}\\regex{}SiteRoom.json'.format(os.path.dirname(__file__), tenantName)
    pathToRegexfileRackOnly = '{}\\regex{}Rack.json'.format(os.path.dirname(__file__), tenantName)
    file_exists3 = exists(pathToRegexfileSiteRoomOnly)
    file_exists2 = exists(pathToRegexfileRackOnly)

    if not file_exists2 or not file_exists3:
        print("\nCannot find one or all regex file")
        return
    print("Before API GET at time: {}".format(time.time()))
    siteAvailable = API_GET.GetSitesNames(tenantName, url, headers)
    if not siteAvailable:
        print("\nThe tenant name is wrong or there are no available sites for this tenant")
        return
    print("After API GET at time: {}".format(time.time()))
    results = OCR.PerformOCR(img, 'easyocr')
    print("after OCR at time: {}".format(time.time()))
    for (bbox, text, prob) in results:
        print(text)

    for (bbox, text, prob) in results:
        text = OCR.ReplaceSymbol(text)

        #Check if the label is full
        if len(text) > 6 and not isNextTextRack:
            isLabelSeparated = False

        #Case where the label is full (Site + Room + Rack), we can do all the processing with one text
        if not isLabelSeparated:
            site, room, output = OCR.RecoverSiteRoom(text, bbox, img, pathToRegexfileSiteRoomOnly, siteAvailable)
            # print("\n Site = {} \n Room = {} \n Rack = {} SR non-separ".format(site, room, rack))
            if site is not None and room is not None:
                rack, output = OCR.RecoverRack(text, bbox, img, pathToRegexfileRackOnly)
                print("\n Site = {} \n Room = {} \n Rack = {} R non-separ".format(site, room, rack))
                OCR.DisplayImage(output)
                return
            isLabelSeparated = True

        #Case where the label is seperated in 2 text
        else:
            if not isNextTextRack:
                site, room, output = OCR.RecoverSiteRoom(text, bbox, img, pathToRegexfileSiteRoomOnly, siteAvailable)
                # print("\n Site = {} \n Room = {} \n Rack = {} SR separ".format(site, room, rack))
            if isNextTextRack:
                rack, output = OCR.RecoverRack(text, bbox, img, pathToRegexfileRackOnly)
                print("\n Site = {} \n Room = {} \n Rack = {} R separ".format(site, room, rack))
                if rack is not None:
                    OCR.DisplayImage(output)
                    return
            if site is not None and room is not None:
                isNextTextRack = True
    print("After post processing at time: {}".format(time.time()))
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




