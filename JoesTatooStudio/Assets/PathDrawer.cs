using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathDrawer : MonoBehaviour
{
    public List<PathCreator> comparePaths = new List<PathCreator>();
    List<PathCreator> playerPaths = new List<PathCreator>();
    public GameObject playerPathPrefab;

    public Text text;

    public float minDistance = 0.1f;

    int score = 0;

    Camera cam;
    Vector3 lastPos;
    List<Vector3> vertices = new List<Vector3>();
    float maxDist;

    LineRenderer lineRenderer;
    PathCreator pathCreator;
    
    private void Start()
    {
        for (int i = 0; i < comparePaths.Count; i++)
        {
            maxDist = comparePaths[i].path.length;
        }

        maxDist *= 1.025f;

        cam = Camera.main;
    }

    void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            lastPos = mousePos;

            var playerPath = Instantiate(playerPathPrefab, transform);

            lineRenderer = playerPath.GetComponent<LineRenderer>();
            pathCreator = playerPath.GetComponent<PathCreator>();

            playerPaths.Add(pathCreator);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, lastPos);
        
            vertices.Add(lastPos);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 newPos = mousePos;
            var dist = Vector2.Distance(lastPos, newPos);

            if (dist > minDistance)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPos);
                maxDist -= dist;
                lastPos = newPos;

                vertices.Add(lastPos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            var bPath = new BezierPath(vertices, false, PathSpace.xy);

            pathCreator.bezierPath = bPath;            
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            CheckPaths();
        }
    }

    void CheckPaths ()
    {
        int totalSamples = 0;

        for (int j = 0; j < comparePaths.Count; j++)
        {
            var comparePath = comparePaths[j];
            int samples = comparePath.path.NumPoints * 2;

            totalSamples += samples;

            for (int i = 0; i < samples; i++)
            {
                var pointA = comparePath.path.GetPointAtTime(1 * ((1 + i) / (float)samples), EndOfPathInstruction.Stop);
                var dist = 999f;

                foreach (var p in playerPaths)
                {
                    var newDist = Vector3.Distance(pointA, p.path.GetClosestPointOnPath(pointA));

                    if (newDist < dist)
                    {
                        if (newDist < 0.1f)
                        {
                            score++;
                            break;
                        }
                        dist = newDist;
                    }
                }
            }
        }

        text.text = ((int)((float)score / totalSamples * 100.0f)).ToString();
    }
}
