using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class English 
 {
    public static string EN_001 
    { 
        get 
        { 
            return "OGrEE-3D AR";
        }
    }

    public static string EN_002 
    { 
        get 
        { 
            return "Welcome to the AR client of OGrEE-3D.'\n'In this application, you will take a picture of rack label to display its digital twin.'\n'Please click on 'Confirm' to continue";
        }
    }

    public static string EN_003
    { 
        get 
        { 
            return "Confirm";
        }
    }

    public static string EN_004
    { 
        get 
        { 
            return "Found Rack";
        }
    }

    public static string EN_005(string site, string room, string rack)
    { 
        return $"Please click on 'Confirm' to place the rack {site}{room}-{rack}. '\n'Click on 'Cancel' if the label was misread or if you want to take another picture.";
    }
 }
