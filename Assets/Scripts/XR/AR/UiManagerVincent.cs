using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI;
using System.Threading.Tasks;
using System;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.UI;

public class UiManagerVincent : MonoBehaviour
{
    public GameObject buttonPicture;
    public TextMeshPro apiResponseTMP;
    public static UiManagerVincent instance;
    [SerializeField][Tooltip("Assign DialogLarge_192x192.prefab")] private GameObject dialogPrefabLarge;
    [SerializeField][Tooltip("Assign DialogLarge_192x192.prefab")] private GameObject dialogPrefabMedium;
    public Dialog dialogHelp;
    public Dialog dialogPhoto;

    public Canvas canvas;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


#if VR
    public void UpdateText(GameObject _g, string _text)
    {
        TextMeshPro tmp = _g.GetComponent<TextMeshPro>();
        tmp.text = _text;
    }

    public void SetCanvasAsParent(GameObject _obj)
    {
    }

    public void ChangeButtonColor(GameObject _button, Color _color)
    {
        _button.transform.Find("BackPlate").GetChild(0).GetComponent<Renderer>().material.color = _color;
    }

    public void DeactivateButtonAndText()
    {
        buttonPicture.SetActive(false);
        apiResponseTMP.gameObject.SetActive(false);
    }

    public void UpdateText(string _text)
    {
        apiResponseTMP.gameObject.SetActive(true);
        apiResponseTMP.text = _text;
    }

    public async Task EnableDialogApiListener()
    {
        dialogPhoto = Dialog.Open(dialogPrefabMedium, DialogButtonType.Confirm | DialogButtonType.Cancel, "Found Rack !", $"Please click on 'Confirm' to place the rack {ApiListener.instance.site}{ApiListener.instance.room}-{ApiListener.instance.rack}.\nClick on 'Cancel' if the label was misread or if you want to take another picture.", true);
        ConfigureDialog(dialogPhoto);
        while (dialogPhoto.State != DialogState.Closed)
        {
            await Task.Delay(100);
        }
    }

    ///<summary>
    /// Configure A Dialog with fixed parameters
    ///</summary>
    ///<param name="_myDialog"> The Dialog box to update</param>
    public void ConfigureDialog(Dialog _myDialog)
    {
        _myDialog.GetComponent<Follow>().MinDistance = 0.5f;
        _myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
        _myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
        _myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
    }

    public void EnableDialogHelp()
    {
        dialogHelp = Dialog.Open(dialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
    }

    public void AttachSettingsToMainMenu(GameObject _obj, GameObject _mainMenu)
    {
        _obj.SetActive(false);
        _obj.GetComponent<Follow>().enabled = false;
        _obj.transform.SetParent(_mainMenu.transform);
        _obj.transform.localPosition = new Vector3(0.02f, -0.08f, 0);
        _obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Destroy(_obj.transform.Find("Backplate").gameObject);
    }

    public void ChangeButtonText(GameObject _button, string _text)
    {
        _button.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = _text;
    }

    public UnityEngine.Events.UnityEvent ButtonOnClick(GameObject _g)
    {
        return _g.GetComponent<ButtonConfigHelper>().OnClick;
    }

    public void UpdateGrid(GridObjectCollection _gridCollection)
    {
        _gridCollection.UpdateCollection();
    }

    public GridObjectCollection GetGrid(GameObject _parentList)
    {
        return _parentList.transform.GetComponent<GridObjectCollection>();
    }

    public int ReturnGridNumberOfRows(GridObjectCollection _gridCollection)
    {
        return _gridCollection.Rows;
    }
#endif



#if !VR
    public void UpdateText(GameObject _g, string _text)
    {
        TextMeshProUGUI tmp = _g.GetComponent<TextMeshProUGUI>();
        tmp.text = _text;
    }

    public void SetCanvasAsParent(GameObject _obj)
    {
        _obj.transform.SetParent(canvas.transform);
    }

    public void ChangeButtonColor(GameObject _button, Color _color)
    {
        _button.GetComponent<Image>().color = _color;
    }

    public void DeactivateButtonAndText()
    {
        buttonPicture.SetActive(false);
        apiResponseTMP.gameObject.SetActive(false);
    }

    public void UpdateText(string _text)
    {
        apiResponseTMP.gameObject.SetActive(true);
        apiResponseTMP.text = _text;
    }

    public async Task EnableDialogApiListener()
    {
        dialogPhoto = Dialog.Open(dialogPrefabMedium, DialogButtonType.Confirm | DialogButtonType.Cancel, "Found Rack !", $"Please click on 'Confirm' to place the rack {ApiListener.instance.site}{ApiListener.instance.room}-{ApiListener.instance.rack}.\nClick on 'Cancel' if the label was misread or if you want to take another picture.", true);
        ConfigureDialog(dialogHelp);
        while (dialogHelp.State != DialogState.Closed)
        {
            await Task.Delay(100);
        }
    }

    ///<summary>
    /// Configure A Dialog with fixed parameters
    ///</summary>
    ///<param name="_myDialog"> The Dialog box to update</param>
    public void ConfigureDialog(Dialog _myDialog)
    {
        _myDialog.GetComponent<Follow>().MinDistance = 0.5f;
        _myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
        _myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
        _myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
    }

    public void EnableDialogHelp()
    {
        dialogHelp = Dialog.Open(dialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
    }

    public void AttachSettingsToMainMenu(GameObject _obj, GameObject _mainMenu)
    {
        _obj.SetActive(false);
        _obj.GetComponent<Follow>().enabled = false;
        _obj.transform.SetParent(_mainMenu.transform);
        _obj.transform.localPosition = new Vector3(0.02f, -0.08f, 0);
        _obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Destroy(_obj.transform.Find("ButtonClose").gameObject);
        Destroy(_obj.transform.Find("Backplate").gameObject);
    }

    public void ChangeButtonText(GameObject _button, string _text)
    {
        _button.GetComponentInChildren<TMP_Text>().text = _text;
    }

    public UnityEngine.Events.UnityEvent ButtonOnClick(GameObject _g)
    {
        return _g.GetComponent<Button>().onClick;
    }

    public void UpdateGrid(GridLayoutGroup _gridCollection)
    {
    }

    public GridLayoutGroup GetGrid(GameObject _parentList)
    {
        return _parentList.transform.GetComponent<GridLayoutGroup>();
    }

    public int ReturnGridNumberOfRows(GridLayoutGroup _gridCollection)
    {
        return _gridCollection.constraintCount;
    }

#endif
}
