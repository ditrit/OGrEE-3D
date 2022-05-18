using UnityEngine;
using TMPro;
using System.Threading.Tasks;
/*
The "easiest" and most elegant way I can think about is using a Vertical Layout Group

Add a new empty gameobject under your canvas
Set the desired dimensions using the RectTransform component
Attach the "Vertical Layout Group" component
In your code, for each string in your list :
Create a new GameObject :
Attach a text component to it
Fill the text attribute with your string
Set the parent of the transform to be the first empty gameobject in 1st step
Here is a piece of code I haven't tested :*/

// Drag & Drop the vertical layout group here
/*public UnityEngine.UI.VerticalLayoutGroup verticalLayoutGroup ;

// ... In your function
RectTransform parent = verticalLayoutGroup.GetComponent<RectTransform>() ;
for( int index = 0 ; index < stringList.Count ; ++index )
{
     GameObject g = new GameObject( stringList[index] ) ;
     UnityEngine.UI.Text t = g.AddComponent<UnityEngine.UI.Text>();
     t.addComponent<RectTransform>().setParent( parent ) ;
     t.text = stringList[index] ;
}
/*
If you need more customization, you can instantiate a prefab instead of
manually create the texts gameobjects.*/

public class APIResponseAsLIst : MonoBehaviour
{
    private string currentHost = "";
    public string maisonHost = "192.168.1.38";
    public string telephoneHost = "192.168.229.231";
    public string customer = "EDF";
    public string site = "NOE";
    private string building;
    private string room;
    private string rack;
    private string customerAndSite;
    public string port = "5000";
    public TextMeshPro apiResponseTMP = null;

    // Start is called before the first frame update
    private async Task Start()
    {
        await Task.Delay(1000);
        //ApiManager.instance.CreateGetRequest("EDF");
        await ApiManager.instance.GetObjectVincent("tenants/EDF/sites");
        //ApiManager.instance.GetHttpData();
    }

    
}
