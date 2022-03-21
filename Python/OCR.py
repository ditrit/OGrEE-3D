import cv2
import easyocr

#Configuration langage of the OCR, use norwegian to have the Ø symbol
reader = easyocr.Reader(['da', 'no'])

def cleanup_text(text):
	#strip out non-ASCII text so we can draw the text on the image using OpenCV
	return "".join([c if ord(c) < 128 else "" for c in text]).strip()

def PerformOCR(img):
    #perform OCR on the image provided
    results = reader.readtext(img)
    return results

def DrawBoundingBoxAddText(img, bbox, text):
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
    cv2.putText(img, text, (tl[0], tl[1] - 10), cv2.FONT_HERSHEY_SIMPLEX, 2, (0, 255, 0), 4)

    return output

def DisplayImage(output):
    #Display the image provided
    cv2.namedWindow("Output", cv2.WINDOW_NORMAL)
    cv2.resizeWindow('Output', 1000, 1000)
    cv2.imshow("Output", output)
    cv2.waitKey(0)
    return

def ReplaceSymbol(text):
    #Remove blank spaces in the text and turn Ø into 0
    text = text.replace(" ", "")
    text = text.replace("Ø", "0")
    return text

def RecoverSiteRoomRack(text, prob, bbox, img, siteAvailable):
    #parse the text provided into 3 variables site, room, rack if possible
    site, room, rack = None, None, None
    output = img
    if text[:3] in siteAvailable:
        if len(text) >= 8:
            # print("[INFO] {:.4f}: {}".format(prob, text))
            output = DrawBoundingBoxAddText(img, bbox, text)
            site = text[:3]
            room = text[3:5]
            rack = text[6:]
            print("\n Site = {} \n Room = {} \n Rack = {}".format(site, room, rack))
            return site, room, rack, output
        else:
            print("\nLabel is too short")
            print("The label read was: {}".format(text))
            return site, room, rack, output
    else:
    #     print("\nLabel is incorrect, it should start with 'NOE' or 'PCY'")
    #     print("The label read was :{}".format(text))
        return site, room, rack, output