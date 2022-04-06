import requests

#####################################################################################################################
#####################################################################################################################

def GetSitesNames(customer, url, headers):
    # Return available sites for a given tenants on a designated DB(url)
    siteNames = []
    apiURL = "http://{}:3001/api/tenants/{}/sites".format(url, customer)
    payload = ""
    response = requests.request("GET", apiURL, headers=headers, data=payload)
    test = response.json()['data']['objects']
    #Check if the response is false
    if not test:
        return siteNames
    #else the response is true
    else:
        for i in range(len(test)):
            name = response.json()['data']['objects'][i]['name']
            if name not in siteNames:
                siteNames.append(name)
        print("\nAvailable sites for the tenant: {} are: {}".format(customer, siteNames))
        return siteNames

#####################################################################################################################
#####################################################################################################################
