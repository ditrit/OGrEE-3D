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
    public static APIResponseAsLIst instance;
    private string tenant;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    private async Task Start()
    {
        await Task.Delay(1000);
        //ApiManager.instance.CreateGetRequest("EDF");
        ApiManager.instance.GetObjectVincent("tenants/EDF/sites", tenant);
        //ApiManager.instance.GetHttpData();
    }

    public void InitializeTenant(string _tenant)
    {
        tenant = _tenant;
    }

    public void ToggleParentListAndButtons(GameObject parentListAndButtons)
    {
        if (parentListAndButtons.activeSelf)
        {
            parentListAndButtons.SetActive(false);
        }
        else
        {
            parentListAndButtons.SetActive(true);
            ApiManager.instance.GetObjectVincent("tenants/EDF/sites", tenant);
        }
    }
}
