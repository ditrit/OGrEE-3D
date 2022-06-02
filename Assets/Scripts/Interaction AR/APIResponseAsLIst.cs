using UnityEngine;
using TMPro;
using System.Threading.Tasks;

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
        await Task.Delay(100);
        ApiManager.instance.GetObjectVincent($"tenants/{tenant}/sites", tenant);
        transform.parent.gameObject.SetActive(false);
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
