using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vanilla.Easing;

public class Tile_Group_Unavailable : Tile_Element
{

	public CanvasGroup group;
	
	void Start()
	{
		tile.available.onInactiveNormalStart  += () => group.gameObject.SetActive(true);
		tile.available.onActiveNormalComplete += () => group.gameObject.SetActive(false);

		tile.available.onActiveNormalFrame += n => group.alpha = (1.0f - n).InOutQuadratic();

		tile.available.onInactiveNormalFrame += n => group.alpha = (1.0f - n).InOutQuadratic();

		if (!tile.experience.ContentFullyDownloaded)
		{
			group.gameObject.SetActive(true);
			group.alpha = 1.0f;
		}
	}

}