import os
from flask import Flask, request, send_from_directory
from gevent.pywsgi import WSGIServer
import print_Hello_World, main

IP = "172.30.139.155"
PORT = 6000

# set the project root directory as the static folder
app = Flask(__name__)

@app.route('/', methods=['POST'])
def DownloadFile():
    # request.form to get form parameter
    img = request.files["Label_Rack"].read()
    tenantName = request.form["Tenant_Name"]
    site, room, rack = print_Hello_World.main(img, tenantName)
    if site is not None and room is not None and rack is not None:
        json = site + room + '-' + rack
    if site is None or room is None or rack is None:
        json = "Could not find rack label on the picture, please try again"
    # outF = open("Label_rack.png", "wb")
    # outF.write(vidFile)

    return json

if __name__ == "__main__":
    http_server = WSGIServer((IP, PORT), app)
    http_server.serve_forever()