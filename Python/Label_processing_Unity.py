import sys

import OCR
from os.path import exists
import os.path
import os
import ShapeDetector
import time
import re

import Utils

path = os.path.dirname(__file__)

#####################################################################################################################
#####################################################################################################################

def main(img, site, regexp, type, isCropped):
    #Initialize var site, room, rack to avoid exceptions
    room, rack = None, None

    # Flag for detecting if the next text is supposed to be the rack
    isNextTextRack = False

    #Flag for detecting if the label is separated for Site+Room and Rack
    isLabelSeparated = True

    #Split the regexp into 2 regex: Site+Room and rack
    if type == 'rack':
        regexSiteRoom, regexRack = Utils.RegexSiteRoomRackSpliter(regexp)
    else:
        print("Did not implement code for non rack object yet")
        sys.exit()

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


    labelMatcherSiteRoom = re.compile(regexSiteRoom)
    labelMatcherRack = re.compile(regexRack)

    for (bbox, text, prob) in results:
        text = OCR.ReplaceSymbol(text)

        # Check if the label is full
        if len(text) > 6 and not isNextTextRack:
            isLabelSeparated = False

        # Case where the label is full (Site + Room + Rack), we can do all the processing in one go (one text)
        if not isLabelSeparated:
            site, room, output = OCR.RecoverSiteRoom(text, img, labelMatcherSiteRoom, site)
            print("\n Site = {} \n Room = {} \n Rack = {} (SR non-separ)".format(site, room, rack))
            if site is not None and room is not None:
                rack, output = OCR.RecoverRack(text, img, labelMatcherRack)
                print("\n Site = {} \n Room = {} \n Rack = {} (R non-separ)".format(site, room, rack))
                OCR.DisplayImage(imgAndtext)
                print("\nPerformed rack label detection and correction in: {} s".format(time.time() - current))
                return site, room, rack
            isLabelSeparated = True

        # Case where the label is seperated in 2 text
        else:
            if not isNextTextRack:
                site, room, output = OCR.RecoverSiteRoom(text, img, labelMatcherSiteRoom, site)
                print("\n Site = {} \n Room = {} \n Rack = {} (SR separ)".format(site, room, rack))
            if isNextTextRack:
                rack, output = OCR.RecoverRack(text, img, labelMatcherRack)
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

#####################################################################################################################
#####################################################################################################################

def OCRAndCorrection(img, site, regexp, type, background):
    print("\n\n\nBeginning the processing of the image...")
    json = None
    start = time.time()
    current = start

    #Cropping the image
    croppedImage = ShapeDetector.ShapeAndColorDetector(img, background)
    print("\nCropped image in: {} s".format(time.time() - current))

    # Perform OCR + post-processing on the cropped_image to recover the name of the site, room and rack
    site, room, rack = main(croppedImage, site, regexp, type, True)
    # return label if it was found
    if site is not None and room is not None and rack is not None:
        json = site + room + '-' + rack
        print("\nTotal time: {} s".format(time.time() - start))
        print("\nThe label read is: {}".format(json))
        return json
    else:
        print("\nCould not find rack label on cropped image. Trying on the full image.")
        site, room, rack = main(img, site, regexp, type, False)
        if site is not None and room is not None and rack is not None:
            json = site + room + '-' + rack
            print("\nTotal time: {} s".format(time.time() - start))
            print("\nThe label read is: {}".format(json))
            return json
        if site is None or room is None or rack is None:
            json = "\nCould not find rack label on the picture, please try again"
            print("\nTotal time: {} s".format(time.time() - start))
            print("\nThe label read is: {}".format(json))
            return json
    return json

#####################################################################################################################
#####################################################################################################################




