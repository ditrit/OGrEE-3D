import os
from flask import Flask, request
import API_GET
import Utils
from gevent.pywsgi import WSGIServer
import Label_processing_Unity
import ShapeDetector
import time
import numpy as np
import cv2

IP = '0.0.0.0'
PORT = 5000

# set the project root directory as the static folder
app = Flask(__name__)

#Read API URL and Headers from conf file
pathToConfFile = "{}\\conf.json".format(os.path.dirname(__file__))
url, token, headers = Utils.GetUrlAndToken(pathToConfFile)

@app.route('/', methods=['POST'])
def DownloadFile():
    # request.form to get form parameter
    start = time.time()
    print("\n\nBeginning the processing of the image...")
    print("\nReceived request at time: {}".format(start))
    img = request.files["Label_Rack"].read()
    tenantName = request.form["Tenant_Name"]
    current = time.time()
    print("\nRead file in: {} s".format(current - start))

    #Convert byte-like image into a numpy array, then into an array usable with opencv
    nparr = np.frombuffer(img, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    #Crop the image around the label to reduce processing time for OCR
    cropped_image = ShapeDetector.ShapeAndColorDetector(img)
    print("\nCropped image in: {} s".format(time.time() - current))
    current = time.time()

    #Return the sites that are available for the tenant provided
    siteAvailable = API_GET.GetSitesNames(tenantName, url, headers)
    if not siteAvailable:
        print("\nThe tenant name is wrong or there are no available sites for this tenant")
        return
    print("\nPerformed API GET in: {} s".format(time.time() - current))

    #Perform OCR + post-processing on the cropped_image to recover the name of the site, room and rack
    site, room, rack = Label_processing_Unity.main(cropped_image, tenantName, siteAvailable, True)

    #return label if it was found
    if site is not None and room is not None and rack is not None:
        json = site + room + '-' + rack
        print("\nTotal time: {} s".format(time.time() - start))
        print("\nThe label read is: {}".format(json))
        return json
    else:
        print("\nCould not find rack label on cropped image. Trying on the full image.")
        site, room, rack = Label_processing_Unity.main(img, tenantName, siteAvailable, False)
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

@app.route('/', methods=['GET'])
def returnToto():
    return "connected to the flaskAPI"


if __name__ == "__main__":

    # app.run(host=IP, port=PORT, threaded = True)
    # #
    http_server = WSGIServer((IP, PORT), app)
    http_server.serve_forever()