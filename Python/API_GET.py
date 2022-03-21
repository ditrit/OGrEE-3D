import requests
import json


def GetSitesNames(tenantName, url, headers):
    # Return available sites for a given tenants on a designated DB(url)
    siteName = []
    apiURL = "http://{}:3001/api/tenants/{}/sites".format(url, tenantName)
    payload = ""
    response = requests.request("GET", apiURL, headers=headers, data=payload)
    test = response.json()['data']['objects']
    #Check if the response is false
    if not test:
        return siteName
    #else the response is true
    else:
        for i in range(len(test)):
            name = response.json()['data']['objects'][i]['name']
            if name not in siteName:
                siteName.append(name)
        print("\nAvailable sites for the tenant: {} are: {}".format(tenantName, siteName))
        return siteName

