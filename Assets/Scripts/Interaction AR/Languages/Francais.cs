using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class Francais 
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
            return "Bienvenue dans le client AR of OGrEE-3D.'\n'Dans ce module, vous allez prendre en photo l'étiquette d'un rack pour en afficher le jumeau numérique.'\n'Cliquer sur 'Confirmation' pour continuer";
        }
    }

    public static string EN_003
    { 
        get 
        { 
            return "Confirmation";
        }
    }

    public static string EN_004
    { 
        get 
        { 
            return "Rack trouvé";
        }
    }

    public static string EN_005(string site, string room, string rack)
    { 
        return $"Cliquer sur 'Confirm' pour placer le rack {site}{room}-{rack}. '\n'Cliquez sur 'Cancel' si l'étiquette a mal été lue et que vous souhaitez reprendre une photo.";
    }
 }
