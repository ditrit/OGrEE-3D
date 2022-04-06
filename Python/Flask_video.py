import os
from flask import Flask, request
import API_GET
import Utils
from gevent.pywsgi import WSGIServer
import Label_processing_Unity
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

#####################################################################################################################
#####################################################################################################################

@app.route('/', methods=['POST'])
def DownloadFile():
    # request.form to get form parameter
    print("\n\nBeginning the processing of the image...")
    start = time.time()

    img = request.files["Label_Rack"].read()
    tenantName = request.form["Tenant_Name"]
    current = time.time()
    print("\nRead file in: {} s".format(current - start))

    #Convert byte-like image into a numpy array, then into an array usable with opencv
    nparr = np.frombuffer(img, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    #Return the sites that are available for the tenant provided
    siteAvailable = API_GET.GetSitesNames(tenantName, url, headers)
    if not siteAvailable:
        print("\nThe tenant name is wrong or there are no available sites for this tenant")
        return
    print("\nPerformed API GET in: {} s".format(time.time() - current))

    json = Label_processing_Unity.OCRAndCorrection(img, tenantName, siteAvailable)
    return json

#####################################################################################################################
#####################################################################################################################

@app.route('/', methods=['GET'])
def returnToto():
    return "connected to the flaskAPI"

#####################################################################################################################
#####################################################################################################################

if __name__ == "__main__":

    # app.run(host=IP, port=PORT, threaded = True)
    # #
    http_server = WSGIServer((IP, PORT), app)
    http_server.serve_forever()