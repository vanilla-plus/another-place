using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class Tile_LocalState_Text : Tile_Element
{

	public TextMeshProUGUI element;

	void Start()
	{
		tile.experience.onContentAvailabilityChange += HandleAvailabilityChange;

		tile.experience.onDownloadPacket += (bytes,
		                                     normal) => element.text = $"{Mathf.FloorToInt(normal * 100.0f)}%";
	}
	
	private void HandleAvailabilityChange(bool available) => element.text = available ? "READY" : "0%";

	
}
