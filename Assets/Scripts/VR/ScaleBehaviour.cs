using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBehaviour : MonoBehaviour
{
    private Microsoft.MixedReality.Toolkit.Utilities.Solvers.SolverHandler solver;
    bool done = false;

    private void Start()
    {
        solver = GetComponent<Microsoft.MixedReality.Toolkit.Utilities.Solvers.SolverHandler>();
    }

    public void ResetScale(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (solver.UpdateSolvers)
        {
            if (!done)
            {
                ResetScale(gameObject);
                done = true;
            }
        }
        else
        {
            if (done)
            {
                done = false;
            }
        }
    }
}
