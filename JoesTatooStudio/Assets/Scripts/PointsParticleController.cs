using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PointsParticleController : MonoBehaviour
{
    public float speed = 1f;

    public Sprite goodFace, medFace, badFace;
    public SpriteRenderer spriteRenderer;

    public TextMeshPro scoreText;

    Camera cam;

    private void Start()
    {
        cam = Camera.main;

        Destroy(gameObject, 1f);
    }

    public void SetParticle (Happiness happiness, int score)
    {
        scoreText.text = "+" + score + "$";

        switch (happiness)
        {
            case Happiness.Good:
                spriteRenderer.sprite = goodFace;
                break;
            case Happiness.Med:
                spriteRenderer.sprite = medFace;
                break;
            case Happiness.Bad:
                spriteRenderer.sprite = badFace;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }
}

public enum Happiness { Good, Med, Bad }
