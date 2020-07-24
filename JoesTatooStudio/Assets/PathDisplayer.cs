using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDisplayer : MonoBehaviour
{
    LineRenderer lineRenderer;
    PathCreator pathCreator;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        pathCreator = GetComponent<PathCreator>();

        lineRenderer.positionCount = pathCreator.path.NumPoints;
        lineRenderer.SetPositions(pathCreator.path.localPoints);
    }
}
