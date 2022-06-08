using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class GridMenuHandler : MonoBehaviour
{
    public GameObject parentList;
    public GameObject buttonLeft;
    public GameObject buttonRight;
    public GameObject resulstInfos;
    public GridObjectCollection gridCollection;
    public int numberOfResultsPerPage;
    protected int pageNumber;

    protected delegate void GridButtonDelegate(int buttonIndex);

    protected void UpdateGridDefault(int ObjectNumber, GridButtonDelegate buttonDelegate)
    {

        int maxNumberOfPage = 0;

        if (ObjectNumber % numberOfResultsPerPage == 0)
            maxNumberOfPage = ObjectNumber / numberOfResultsPerPage;
        else
            maxNumberOfPage = ObjectNumber / numberOfResultsPerPage + 1;

        ClearParentList();
        TextMeshPro tmp = resulstInfos.GetComponent<TextMeshPro>();
        tmp.text = $"{ObjectNumber} Results. Page {pageNumber + 1} / {maxNumberOfPage}";
        if (pageNumber > 0)
        {
            buttonLeft.SetActive(true);
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
            {
                pageNumber -= 1;
                UpdateGrid(ObjectNumber, buttonDelegate);
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
                UpdateGrid(ObjectNumber, buttonDelegate);
            });
        }
        else
            buttonRight.SetActive(false);

        int iteratorUpperBound = 0;

        if (pageNumber + 1 == maxNumberOfPage)
            iteratorUpperBound = ObjectNumber - 1;
        else
            iteratorUpperBound = (pageNumber + 1) * numberOfResultsPerPage - 1;

        Debug.Log($"Number of Results: {ObjectNumber}, Page Number: {pageNumber}, MaxNumberOfPage: {maxNumberOfPage}, NumberOfResultsPerPage: {numberOfResultsPerPage}, iteratorUpperBound: {iteratorUpperBound}");
        for (int i = pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
        {
            buttonDelegate(i);
        }

    }

    protected async void ClearParentList()
    {
        for (int i = 0; i < parentList.transform.childCount; i++)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }
        await Task.Delay(1);
        gridCollection.UpdateCollection();
    }

    protected void UpdateGrid(int ObjectNumber, GridButtonDelegate buttonDelegate)
    {
        UpdateGridDefault(ObjectNumber, buttonDelegate);
    }


}
