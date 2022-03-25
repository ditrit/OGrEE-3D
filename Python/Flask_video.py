import os
from flask import Flask, request, send_from_directory
from gevent.pywsgi import WSGIServer
import print_Hello_World, main

IP = "192.168.1.38"
PORT = 6000

# set the project root directory as the static folder
app = Flask(__name__)

@app.route('/', methods=['POST'])
def DownloadFile():
    # request.form to get form parameter
    img = request.files["Label_Rack"].read()
    tenantName = request.form["Tenant_Name"]
    site, room, rack = print_Hello_World.main(img, tenantName)
    json = site + room + '-' + rack
    # outF = open("Label_rack.png", "wb")
    # outF.write(vidFile)

    return json

if __name__ == "__main__":
    http_server = WSGIServer((IP, PORT), app)
    http_server.serve_forever()