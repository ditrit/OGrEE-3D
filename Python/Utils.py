import sys
from os.path import exists
import os
import json
import re

reg = '.+]-'
labelMatcherSpliter = re.compile(reg)
#####################################################################################################################
#####################################################################################################################

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
        print("\nCannot find configuration file (Utils.py)")
        sys.exit()

#####################################################################################################################
#####################################################################################################################

def ReadRegex(pathToRegexFile, customer, site):
    file_exists = exists(pathToRegexFile)
    if file_exists:
        f = open(pathToRegexFile, "r")
        data = json.load(f)
        if data['customer'] == customer:
            attributes = data['regexps']
            for i in range(len(attributes)):
                if attributes[i]['site'] == site:
                    regexp = attributes[i]['regexp']
                    room = attributes[i]['room']
                    type = attributes[i]['type']
                    background = attributes[i]['background']
                    return regexp, room, type, background
            print("site name does not exist (Utils.py)")
            sys.exit()
        else:
            print("customer name is wrong (Utils.py)")
            sys.exit()
    else:
        print("file does not exist (Utils.py)")
        sys.exit()


#####################################################################################################################
#####################################################################################################################

def CustomerAndSiteSpliter(customerAndSite):
    customerSiteList = customerAndSite.split('.')
    customer = customerSiteList[0]
    site = customerSiteList[1]
    return customer, site

#####################################################################################################################
#####################################################################################################################

def RegexSiteRoomRackSpliter(regexp):
    if labelMatcherSpliter.findall(regexp):
        label = labelMatcherSpliter.match(regexp)
        span = label.end()
        siteRoomRegex = regexp[:span-1]
        rackRegex = regexp[span:]
    else:
        print("the regex provided is wrong. Error (Utils.py)")
        sys.exit()

    return siteRoomRegex, rackRegex

#####################################################################################################################
#####################################################################################################################

if __name__ == "__main__":
    RegexSiteRoomRackSpliter("NOEC[1-8]-[AZ][0-9]{2}[b|BIS]?")