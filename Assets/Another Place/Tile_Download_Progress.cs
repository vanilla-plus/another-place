using System.Collections;
using System.Collections.Generic;

using Amazon.DynamoDBv2.Model;

using TMPro;

using UnityEngine;

public class Tile_Download_Progress : Tile_Element
{

	public TextMeshProUGUI element;


	public void Start()
	{
		// Turn on the text element object when the tile is first selected/clicked on
		tile.select.onActiveNormalStart += () => element.gameObject.SetActive(true);

		// Turn off the text element object on the last frame that the tile is fully deselected
		tile.select.onInactiveNormalComplete += () => element.gameObject.SetActive(false);

		tile.experience.onDownloadBegun += HandleDownloadBegun;

		tile.experience.onDownloadPacket += HandleDownloadPacket;

		tile.experience.onDownloadComplete += HandleDownloadComplete;

		element.gameObject.SetActive(false);
		
		if (tile.experience.ContentFullyDownloaded)
		{
			HandleDownloadComplete();
		}
		else
		{
			HandleDownloadBegun();
		}
	}


	private void HandleDownloadBegun() => element.text = "0";


	private void HandleDownloadPacket(ulong bytes,
	                                  float normal) => element.text = Mathf.FloorToInt(normal * 100.0f).ToString();


	private void HandleDownloadComplete() => element.text = string.Empty;

}