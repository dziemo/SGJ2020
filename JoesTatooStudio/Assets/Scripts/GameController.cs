using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public List<GameObject> tattoos;
    public GameObject pointsParticle;

    public Transform tattooParent;
    public Transform tattoReference;
    public TextMeshProUGUI percent;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI endGameText;
    public TextMeshProUGUI loseText;

    public GameObject endRoundPanel;
    public GameObject losePanel;
    public GameObject endGamePanel;
    public TextMeshProUGUI endRoundText;

    public Gradient blackGradient;
    public Animator animator;

    public bool isGameInProgress = false;

    public GameObject arm;

    PathCreator[] templatePaths;
    GameObject currTattoo;
    GameObject tattooTemplate;
    GameObject tattooContainer;

    Camera cam;

    int[] pointsLimits = new int[] { 250, 500, 750, 1000, 1250, 1500, 1750, 2000, 2250, 2500 };

    int maxPoints = 100;

    int round = 0;
    int score = 0;
    int roundTime = 0;
    int points = 0;
    int pointsLimit = 0;
    int totalPoints = 0;

    private void Awake()
    {
        instance = this;

        cam = Camera.main;

        tattoos = new List<GameObject>(Resources.LoadAll("TattoosPrefabs", typeof(GameObject)).Cast<GameObject>());
    }
    
    public void StartRound()
    {
        points = 0;
        pointsLimit = pointsLimits[round];
        round++;
        pointsText.text = points.ToString();
        StartCoroutine(RoundTimer(45));
        SetNewTattoo();

        isGameInProgress = true;
    }

    void SetNewTattoo ()
    {
        arm.GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.45f, 0.45f, 0.85f, 0.85f, 1, 1)); 
        score = 0;

        if (currTattoo)
        {
            Destroy(currTattoo);
            Destroy(tattooTemplate);
        }
        var tempTattoo = tattoos[Random.Range(0, tattoos.Count)];

        maxPoints = tempTattoo.GetComponent<TattooController>().moneyToEarn;
        moneyText.text = maxPoints + "$";

        if (tattooContainer)
        {
            tattoos.Add(tattooContainer);
        }

        tattooContainer = tempTattoo;
        tattoos.Remove(tempTattoo);

        currTattoo = Instantiate(tempTattoo, tattooParent);
        tattooTemplate = Instantiate(tempTattoo, tattoReference);

        LineRenderer[] templateRenderers = currTattoo.GetComponentsInChildren<LineRenderer>();

        var lineRenderers = tattooTemplate.GetComponentsInChildren<LineRenderer>();

        foreach (var lr in lineRenderers)
        {
            lr.colorGradient = blackGradient;
        }

        templatePaths = currTattoo.GetComponentsInChildren<PathCreator>();

        float maxDist = 0f;

        for (int i = 0; i < templatePaths.Length; i++)
        {
            maxDist += templatePaths[i].path.length;
        }

        maxDist *= 1.025f;

        PathDrawer.instance.SetNewTattoo(maxDist, templateRenderers);

        Debug.Log("NEW TATTOO SET");
    }

    public void CheckPaths(List<PathCreator> playerPaths)
    {
        int totalSamples = 0;

        for (int j = 0; j < templatePaths.Length; j++)
        {
            var comparePath = templatePaths[j];
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

        var percents = ((int)((float)score / totalSamples * 100.0f));

        percent.text = percents.ToString() + "%";
        var pointsToAdd = (int)(maxPoints * (percents/100f));
        totalPoints += pointsToAdd;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        var newParticle = Instantiate(pointsParticle, mousePos, Quaternion.identity);

        if (pointsToAdd > maxPoints * 0.75f)
        {
            newParticle.GetComponent<PointsParticleController>().SetParticle(Happiness.Good, pointsToAdd);
        } else if (pointsToAdd > maxPoints * 0.35f)
        {
            newParticle.GetComponent<PointsParticleController>().SetParticle(Happiness.Med, pointsToAdd);
        } else
        {
            newParticle.GetComponent<PointsParticleController>().SetParticle(Happiness.Bad, pointsToAdd);
        }
        points += pointsToAdd;
        pointsText.text = points.ToString() + "$";

        if (roundTime > 0)
        {
            StartCoroutine(WaitForCustomer());
        }
    }

    IEnumerator WaitForCustomer ()
    {
        yield return new WaitForSeconds(1.5f);
        PathDrawer.instance.HideTemplate();
        animator.SetTrigger("End");
        yield return new WaitForSeconds(0.5f);
        if (roundTime > 0)
            SetNewTattoo();
    }

    IEnumerator RoundTimer (int time)
    {
        roundTime = time;

        for (int i = time; i >= 0; i--)
        {
            timer.text = i.ToString();
            yield return new WaitForSeconds(1.0f);
            roundTime = i;
        }

        isGameInProgress = false;

        PathDrawer.instance.OnRoundEnd();
        animator.SetTrigger("End");

        //END ROUND
        if (points >= pointsLimit)
        {
            if (round == 11)
            {
                endGamePanel.SetActive(true);
                endGameText.text = "You earned total of " + totalPoints + "$";
            }
            else
            {
                endRoundPanel.SetActive(true);
                endRoundText.text = "Good job!\nThis round you have to earn " + pointsLimits[round] + "$";
            }
        } else
        {
            losePanel.SetActive(true);
            loseText.text = "You earned total of " + totalPoints + "$";
        }
    }

    public void RestartGame ()
    {
        SceneManager.LoadScene("GameScene");
    }
}
