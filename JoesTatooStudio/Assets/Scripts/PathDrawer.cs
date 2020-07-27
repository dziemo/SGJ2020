using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public static PathDrawer instance;

    public GameObject playerPathPrefab;
    List<PathCreator> playerPaths = new List<PathCreator>();

    public GameObject inkMeter;
    public Transform armTattoo;

    public Animator animator;

    public float minDistance = 0.1f;

    Camera cam;
    Vector3 lastPos;
    List<Vector3> vertices = new List<Vector3>();
    AudioSource tattooSound;
    public AudioSource registerSound;
    LineRenderer lineRenderer;
    LineRenderer[] lineRenderers;
    PathCreator pathCreator;
    
    float currDist = 0, maxDist = 0;
    bool isChecking = false, canDraw = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        tattooSound = GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        
        if (GameController.instance.isGameInProgress && canDraw)
        {

            if (Input.GetMouseButtonDown(0) && currDist > 0)
            {
                lastPos = mousePos;

                var playerPath = Instantiate(playerPathPrefab, transform);

                lineRenderer = playerPath.GetComponent<LineRenderer>();
                pathCreator = playerPath.GetComponent<PathCreator>();

                playerPaths.Add(pathCreator);

                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, lastPos);

                vertices.Add(lastPos);

                tattooSound.Play();
            }

            if (Input.GetMouseButton(0) && currDist > 0)
            {
                Vector3 newPos = mousePos;
                var dist = Vector2.Distance(lastPos, newPos);
                
                if (dist > minDistance && lineRenderer)
                {
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPos);
                    currDist -= dist;
                    lastPos = newPos;

                    vertices.Add(lastPos);
                }
                
                foreach (var lr in lineRenderers)
                {
                    for (int i = 0; i < lr.colorGradient.alphaKeys.Length; i++)
                    {
                        var alpha = Mathf.Lerp(0.2f, 0f, (maxDist - currDist)/maxDist * 1.2f);

                        lr.endColor = new Color(1, 1, 1, alpha);
                        lr.startColor = new Color(1, 1, 1, alpha);
                    }
                }
            }
            else
            {
                tattooSound.Stop();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (vertices.Count > 1)
                {
                    var bPath = new BezierPath(vertices, false, PathSpace.xy);
                    pathCreator.bezierPath = bPath;
                    vertices.Clear();
                }
            }

            if (currDist <= 0)
            {
                FinishTattoo();
            }

            var inkMeterScale = inkMeter.transform.localScale;
            inkMeterScale.y = Mathf.Clamp(currDist / maxDist, 0, 1);
            inkMeter.transform.localScale = inkMeterScale;
        } else
        {
            if (tattooSound.isPlaying)
                tattooSound.Stop();
        }
    }

    public void FinishTattoo ()
    {
        if (!isChecking)
        {
            registerSound.Play();

            if (vertices.Count > 1)
            {
                var bPath = new BezierPath(vertices, false, PathSpace.xy);
                pathCreator.bezierPath = bPath;
                vertices.Clear();
            }

            isChecking = true;
            canDraw = false;
            GameController.instance.CheckPaths(playerPaths);
            
            foreach (var lr in lineRenderers)
            {
                for (int i = 0; i < lr.colorGradient.alphaKeys.Length; i++)
                {
                    lr.endColor = new Color(1, 1, 1, 1);
                    lr.startColor = new Color(1, 1, 1, 1);
                }
            }
        }
    }

    public void HideTemplate ()
    {
        foreach (var lr in lineRenderers)
        {
            foreach (var p in playerPaths)
            {
                p.gameObject.transform.SetParent(armTattoo);
            }

            for (int i = 0; i < lr.colorGradient.alphaKeys.Length; i++)
            {
                lr.endColor = new Color(1, 1, 1, 0);
                lr.startColor = new Color(1, 1, 1, 0);
            }
        }
    }

    public void SetNewTattoo (float dist, LineRenderer[] lRenderers)
    {
        lineRenderers = lRenderers;
        currDist = dist;
        maxDist = currDist;
        isChecking = false;

        for (int i = playerPaths.Count - 1; i >= 0; i--)
        {
            Destroy(playerPaths[i].gameObject);
        }

        playerPaths.Clear();

        animator.SetTrigger("Begin");

        canDraw = true;
    }

    public void OnRoundEnd ()
    {
        FinishTattoo();
        tattooSound.Stop();
    }

}
