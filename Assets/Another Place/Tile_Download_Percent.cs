using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class Tile_Download_Percent : Tile_Element
{

	public TextMeshProUGUI element;

	public void Start()
	{
		// Turn on the text element object when the tile is first selected/clicked on
		tile.select.onActiveNormalStart += () => element.gameObject.SetActive(true);

		// Turn off the text element object on the last frame that the tile is fully deselected
		tile.select.onInactiveNormalComplete += () => element.gameObject.SetActive(false);

		tile.experience.onContentAvailabilityChange += HandleAvailabilityChange;

//		tile.experience.onDownloadBegun  += HandleDownloadBegun;

//		tile.experience.onDownloadPacket += HandleDownloadPacket;

//		tile.experience.onDownloadComplete += HandleDownloadComplete;
//
//		if (tile.experience.ContentFullyDownloaded)
//		{
//			HandleDownloadComplete();
//		}
//		else
//		{
//			HandleDownloadBegun();
//		}
	}

//	private void HandleDownloadBegun()
//	{
////		element.fontSize = 60f;
//		element.text     = "%";
//	}
	
//	private void HandleDownloadPacket(ulong bytes,
//	                                  float normal) => element.text = Mathf.FloorToInt(normal * 100.0f).ToString();
	
//	private void HandleDownloadComplete()
//	{
////		element.fontSize = 38f;
//		element.text     = "READY";
//	}

	

	private void HandleAvailabilityChange(bool available) => element.text = available ? "READY" : "%";

}