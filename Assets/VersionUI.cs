using UnityEngine;
using TMPro;

public class VersionUI : MonoBehaviour
{
    public string wildcard = "*";
    public string format = "v* - ";
    public TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        if (!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        textMesh.text = format.Replace(wildcard, Application.version.ToString());
    }
}
