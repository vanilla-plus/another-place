using UnityEngine;
using UnityEngine.UI;

public class PHUI_Progress_Spinner : MonoBehaviour
{

	public Text  ProgressText;
	public Image FillImage;
	
	private void Awake() => GetComponentInParent<Selection_Tile>().experience.onDownloadPacket += (bytes,
	                                                                                               progress) =>

	                                                                                              {
		                                                                                              FillImage.fillAmount = progress;
		                                                                                              ProgressText.text    = $"{Mathf.FloorToInt(progress * 100.0f)}%";
	                                                                                              };

}