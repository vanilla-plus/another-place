using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class Tile_Download_Percent : Tile_Element
{

	public TextMeshProUGUI element;

	public void Start() => tile.experience.onDownloadPacket += (bytes,
	                                                            normal) => element.text = Mathf.FloorToInt(normal * 100.0f).ToString();

}