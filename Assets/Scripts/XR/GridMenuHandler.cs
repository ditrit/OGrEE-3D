using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using UnityEngine.UI;

//Parent class to build a list menu with an unknow/variable number of items
//Make a new class inheriting from this one and customise your menu here

public class GridMenuHandler : MonoBehaviour
{
    [Tooltip ("Menu needs at least button left, button right, an empty object parentList and a TMP for resulstInfos")]
    public GameObject menuPrefab;
    protected GameObject menu;
    protected GameObject parentList;
    protected GameObject buttonLeft;
    protected GameObject buttonRight;
    protected GameObject resulstInfos;
    #if VR
    protected GridObjectCollection gridCollection;
    #endif
    #if !VR
    protected GridLayoutGroup gridCollection;
    #endif
    protected int numberOfResultsPerPage;
    protected int pageNumber;
    protected int iteratorUpperBound = 0;

    protected delegate void ElementDelegate(int _elementIndex);

    ///<summary>
    /// Builds the list of items in the menu and applies to each displayed item index the function passed in argument
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected void InitializeUIElements()
    {
        menu = Instantiate(menuPrefab, new Vector3(0,0,0.4f) , Quaternion.identity);
        UiManagerVincent.instance.SetCanvasAsParent(menu);
        menu.SetActive(false);
        buttonLeft = menu.transform.Find("ButtonLeft").gameObject;
        if (buttonLeft)
            print("Button left ok");
        else
            print("button left missing");
        buttonRight = menu.transform.Find("ButtonRight").gameObject;
        if (buttonRight)
            print("Button right ok");
        else
            print("button right missing");
        resulstInfos = menu.transform.Find("ResultsInfos").gameObject;
        if (resulstInfos)
            print("result infos ok");
        else
            print("result infos missing");
        parentList = menu.transform.Find("ParentList").gameObject;
        if (parentList)
            print("parent list ok");
        else
            print("parent list missing");
        gridCollection = UiManagerVincent.instance.GetGrid(parentList);
        if (gridCollection)
            print("grid collection ok");
        else
            print("grid collection missing");
    }

    ///<summary>
    /// Builds the list of items in the menu and applies to each displayed item index the function passed in argument
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected void UpdateGridDefault(int _objectNumber, ElementDelegate _elementDelegate)
    {

        int maxNumberOfPage = 0;
        if (_objectNumber % numberOfResultsPerPage == 0)
            maxNumberOfPage = _objectNumber / numberOfResultsPerPage;
        else
            maxNumberOfPage = _objectNumber / numberOfResultsPerPage + 1;

        ClearParentList();
        UiManagerVincent.instance.UpdateText(resulstInfos, $"{_objectNumber} Results. Page {pageNumber + 1} / {maxNumberOfPage}");
        if (pageNumber > 0)
        {
            buttonLeft.SetActive(true);
            var onClickEvent = UiManagerVincent.instance.ButtonOnClick(buttonLeft);
            onClickEvent.RemoveAllListeners();
            onClickEvent.AddListener(() =>
            {
                pageNumber -= 1;
                UpdateGrid(_objectNumber, _elementDelegate);
            });
        }
        else
            buttonLeft.SetActive(false);

        if (pageNumber + 1 < maxNumberOfPage)
        {
            buttonRight.SetActive(true);
            var onClickEvent1 = UiManagerVincent.instance.ButtonOnClick(buttonRight);
            onClickEvent1.RemoveAllListeners();
            onClickEvent1.AddListener(() =>
            {
                pageNumber += 1;
                UpdateGrid(_objectNumber, _elementDelegate);
            });
        }
        else
            buttonRight.SetActive(false);

        if (pageNumber + 1 == maxNumberOfPage)
            iteratorUpperBound = _objectNumber - 1;
        else
            iteratorUpperBound = (pageNumber + 1) * numberOfResultsPerPage - 1;

        Debug.Log($"Number of Results: {_objectNumber}, Page Number: {pageNumber}, MaxNumberOfPage: {maxNumberOfPage}, NumberOfResultsPerPage: {numberOfResultsPerPage}, iteratorUpperBound: {iteratorUpperBound}");
        for (int i = pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
        {
            _elementDelegate(i);
        }

        UiManagerVincent.instance.UpdateGrid(gridCollection);
    }


    ///<summary>
    /// Deletes all displayed items
    ///</summary>
    protected async void ClearParentList()
    {
        for (int i = 0; i < parentList.transform.childCount; i++)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }
        await Task.Delay(1);
        UiManagerVincent.instance.UpdateGrid(gridCollection);
    }


    ///<summary>
    ///Override this function in a child class to add actions to be performed when updating the list display but don't forget to keep the call to UpdateGridDefault
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected void UpdateGrid(int _objectNumber, ElementDelegate _elementDelegate)
    {
        UpdateGridDefault(_objectNumber, _elementDelegate);
    }


}
