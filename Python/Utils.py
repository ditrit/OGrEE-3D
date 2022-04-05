import sys
from os.path import exists

def GetUrlAndToken(pathToConfFile):
    file_exists = exists(pathToConfFile)
    if file_exists:
        f = open(pathToConfFile, "r")
        url = str(f.readline()[:-1])
        token = str(f.readline())
        headers = {
            'Authorization': token
        }
        f.close()
        return url, token, headers
    else:
        print("\nCannot find configuration file")
        sys.exit()