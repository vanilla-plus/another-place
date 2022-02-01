using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class Tile_Download_Icon : Tile_Element
{

	public Image image;

	public Sprite availableIcon;
	public Sprite unavailableIcon;
	
	private void Start() => tile.experience.onContentAvailabilityChange += b => image.sprite = b ? availableIcon : unavailableIcon;

}
