using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

//Parent class to build a list menu with an unknow/variable number of items
//Make a new class inheriting from this one and customise your menu here

public class GridMenuHandler : MonoBehaviour
{
    public GameObject parentList;
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject resulstInfos;
    public GridObjectCollection gridCollection;
    public int numberOfResultsPerPage;
    protected int pageNumber;

    protected delegate void ElementDelegate(int _elementIndex);

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
        TextMeshPro tmp = resulstInfos.GetComponent<TextMeshPro>();
        tmp.text = $"{_objectNumber} Results. Page {pageNumber + 1} / {maxNumberOfPage}";
        if (pageNumber > 0)
        {
            buttonLeft.SetActive(true);
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
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
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
            {
                pageNumber += 1;
                UpdateGrid(_objectNumber, _elementDelegate);
            });
        }
        else
            buttonRight.SetActive(false);

        int iteratorUpperBound = 0;

        if (pageNumber + 1 == maxNumberOfPage)
            iteratorUpperBound = _objectNumber - 1;
        else
            iteratorUpperBound = (pageNumber + 1) * numberOfResultsPerPage - 1;

        Debug.Log($"Number of Results: {_objectNumber}, Page Number: {pageNumber}, MaxNumberOfPage: {maxNumberOfPage}, NumberOfResultsPerPage: {numberOfResultsPerPage}, iteratorUpperBound: {iteratorUpperBound}");
        for (int i = pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
        {
            _elementDelegate(i);
        }

        gridCollection.UpdateCollection();
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
        gridCollection.UpdateCollection();
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
