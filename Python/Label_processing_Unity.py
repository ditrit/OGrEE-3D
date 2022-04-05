import OCR
from os.path import exists
import os.path
import time
import cv2

path = os.path.dirname(__file__)


def main(img, tenantName, siteAvailable, isCropped):
    #Initialize var site, room, rack to avoid exceptions
    site, room, rack = None, None, None

    # Flag for detecting if the next text is supposed to be the rack
    isNextTextRack = False

    #Flag for detecting if the label is separated for Site+Room and Rack
    isLabelSeparated = True

    #Specify the path to the regular expression of the labels
    pathToRegexfile = '{}\\regex{}.json'.format(os.path.dirname(__file__), tenantName)

    #Check if the regexfil exists
    pathToRegexfileSiteRoomOnly = '{}\\regex{}SiteRoom.json'.format(os.path.dirname(__file__), tenantName)
    pathToRegexfileRackOnly = '{}\\regex{}Rack.json'.format(os.path.dirname(__file__), tenantName)
    file_exists3 = exists(pathToRegexfileSiteRoomOnly)
    file_exists2 = exists(pathToRegexfileRackOnly)

    if not file_exists2 or not file_exists3:
        print("\nCannot find one or all regex file")
        return

    #Perform OCR on the img with the specified technology
    current = time.time()
    results = OCR.PerformOCR(img, 'easyocr')
    print("\nperformed OCR in: {} s".format(time.time() - current))
    current = time.time()

    lineNumber = 1
    print("Reading the text on the image...\n")
    imgAndtext = img.copy()
    for (bbox, text, prob) in results:
        if isCropped:
            OCR.DrawBoundingBoxAddTextCropped(imgAndtext, bbox, text)
        else:
            OCR.DrawBoundingBoxAddTextNoCropped(imgAndtext, bbox, text)
        print("Read line {}: {}".format(lineNumber, text))
        lineNumber += 1

    for (bbox, text, prob) in results:
        text = OCR.ReplaceSymbol(text)

        # Check if the label is full
        if len(text) > 6 and not isNextTextRack:
            isLabelSeparated = False

        # Case where the label is full (Site + Room + Rack), we can do all the processing in one go (one text)
        if not isLabelSeparated:
            site, room, output = OCR.RecoverSiteRoom(text, bbox, img, pathToRegexfileSiteRoomOnly, siteAvailable)
            print("\n Site = {} \n Room = {} \n Rack = {} (SR non-separ)".format(site, room, rack))
            if site is not None and room is not None:
                rack, output = OCR.RecoverRack(text, bbox, img, pathToRegexfileRackOnly)
                print("\n Site = {} \n Room = {} \n Rack = {} (R non-separ)".format(site, room, rack))
                OCR.DisplayImage(imgAndtext)
                print("\nPerformed rack label detection and correction in: {} s".format(time.time() - current))
                return site, room, rack
            isLabelSeparated = True

        # Case where the label is seperated in 2 text
        else:
            if not isNextTextRack:
                site, room, output = OCR.RecoverSiteRoom(text, bbox, img, pathToRegexfileSiteRoomOnly, siteAvailable)
                print("\n Site = {} \n Room = {} \n Rack = {} (SR separ)".format(site, room, rack))
            if isNextTextRack:
                rack, output = OCR.RecoverRack(text, bbox, img, pathToRegexfileRackOnly)
                print("\n Site = {} \n Room = {} \n Rack = {} (R separ)".format(site, room, rack))
                if rack is not None:
                    OCR.DisplayImage(imgAndtext)
                    print("\nPerformed rack label detection and correction in: {} s".format(time.time() - current))
                    return site, room, rack
            if site is not None and room is not None:
                isNextTextRack = True

    print("\nPerformed rack label detection and correction in: {} s".format(time.time() - current))

    OCR.DisplayImage(imgAndtext)
    return site, room, rack






