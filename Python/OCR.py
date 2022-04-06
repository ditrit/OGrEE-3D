import cv2
import easyocr
import pytesseract
import re

# Configuration langage of the OCR, use norwegian to have the Ø symbol
reader = easyocr.Reader(['no', 'fr'])
pytesseract.pytesseract.tesseract_cmd = 'C:\\Program Files\\Tesseract-OCR\\tesseract.exe'

#####################################################################################################################
#####################################################################################################################

def cleanup_text(text):
    # strip out non-ASCII text so we can draw the text on the image using OpenCV
    return "".join([c if ord(c) < 128 else "" for c in text]).strip()

#####################################################################################################################
#####################################################################################################################

def PerformOCR(img, method):
    # perform OCR on the image provided
    if method == 'easyocr':
        results = reader.readtext(img)
        return results
    if method == 'tesseract':
        # img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        boxes = pytesseract.image_to_data(img)
        text = pytesseract.image_to_string(img)
        # print(boxes)
        print(text)
        return

#####################################################################################################################
#####################################################################################################################

def DrawBoundingBoxAddTextCropped(img, bbox, text):
    # unpack the bounding box
    (tl, tr, br, bl) = bbox
    tl = (int(tl[0]), int(tl[1]))
    tr = (int(tr[0]), int(tr[1]))
    br = (int(br[0]), int(br[1]))
    bl = (int(bl[0]), int(bl[1]))

    # cleanup the text and draw the box surrounding the text along
    # with the OCR'd text itself
    text = cleanup_text(text)
    output = cv2.rectangle(img, tl, br, (0, 255, 0), 2)
    cv2.putText(img, text, (bl[0], bl[1] - 10), cv2.FONT_HERSHEY_SIMPLEX, 2, (0, 255, 0), 4)

    return output

#####################################################################################################################
#####################################################################################################################

def DrawBoundingBoxAddTextNoCropped(img, bbox, text):
    # unpack the bounding box
    (tl, tr, br, bl) = bbox
    tl = (int(tl[0]), int(tl[1]))
    tr = (int(tr[0]), int(tr[1]))
    br = (int(br[0]), int(br[1]))
    bl = (int(bl[0]), int(bl[1]))

    # cleanup the text and draw the box surrounding the text along
    # with the OCR'd text itself
    text = cleanup_text(text)
    output = cv2.rectangle(img, tl, br, (0, 255, 0), 2)
    cv2.putText(img, text, (tl[0], tl[1] - 10), cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 0, 0), 4)

    return output

#####################################################################################################################
#####################################################################################################################

def DisplayImage(img):
    # Display the image provided
    cv2.namedWindow("Output", cv2.WINDOW_NORMAL)
    cv2.resizeWindow('Output', 1000, 1000)
    cv2.imshow("Output", img)
    cv2.waitKey(0)
    return

#####################################################################################################################
#####################################################################################################################

def ReplaceSymbol(text):
    # Remove blank spaces in the text and turn Ø into 0
    text = text.replace(" ", "")
    text = text.replace("Ø", "0")
    return text

#####################################################################################################################
#####################################################################################################################

def RecoverRack(text, img, labelMatcher):
    # parse the text provided into 3 variables site, room, rack if possible
    rack = None
    output = img
    firstCharacterOfRackLabel = text[0]
    firstCharacterOfRackLabel = firstCharacterOfRackLabel.replace("0", "Q")
    firstCharacterOfRackLabel = firstCharacterOfRackLabel.replace("O", "Q")

    correctLabel = firstCharacterOfRackLabel
    for i in range(1, len(text)):
        characterToReplace = text[i]
        characterToReplace = characterToReplace.replace("O", "0")
        characterToReplace = characterToReplace.replace("o", "0")
        characterToReplace = characterToReplace.replace("Q", "0")
        correctLabel += characterToReplace

    correctLabel = correctLabel.replace("-0", "-Q")
    correctLabel = correctLabel.replace("-O", "-Q")

    if labelMatcher.findall(correctLabel):
        rackLabel = labelMatcher.findall(correctLabel)[0]
        rack = rackLabel
    return rack, output

#####################################################################################################################
#####################################################################################################################

def RecoverSiteRoom(text, img, labelMatcher, site):
    # parse the text provided into 3 variables site, room, rack if possible
    room = None
    output = img

    text = text.replace("I", "1")
    text = text.replace("T", "1")
    text = text.replace("CS", "C5")

    if labelMatcher.findall(text):
        siteRoomLabel = labelMatcher.findall(text)[0]
        if site == "NOE":
            room = siteRoomLabel[3:5]
        elif site == "PCY":
            room = siteRoomLabel[3]
    return site, room, output

#####################################################################################################################
#####################################################################################################################
