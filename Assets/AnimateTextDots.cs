using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTextDots : MonoBehaviour
{
    public TMPro.TextMeshProUGUI textMesh;
    public int minimumDots = 1, maximumDots = 3;
    public float timeBetweenDots = 0.5f;

    string originalText;
    int numberOfDots;
    float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        originalText = textMesh.text;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (time > timeBetweenDots)
        {
            time -= timeBetweenDots;

            if (++numberOfDots > maximumDots) numberOfDots = minimumDots;

            string text = originalText;
            for (int i = 0; i < numberOfDots; i++) text += ".";

            textMesh.text = text;
        }
    }
}
