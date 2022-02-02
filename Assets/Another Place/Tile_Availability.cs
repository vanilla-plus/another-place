using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Vanilla.Easing;

public class Tile_Availability : Tile_Element
{

//	public Tile_State contentAvailable;

	public CanvasGroup availableGroup;
	public CanvasGroup unavailableGroup;


	private void Start()
	{
		tile.available.onActiveNormalStart      += () => availableGroup.gameObject.SetActive(true);
		tile.available.onInactiveNormalComplete += () => availableGroup.gameObject.SetActive(false);

		tile.available.onInactiveNormalStart  += () => unavailableGroup.gameObject.SetActive(true);
		tile.available.onActiveNormalComplete += () => unavailableGroup.gameObject.SetActive(false);

		tile.available.onActiveNormalFrame += n =>
		                                      {
			                                      availableGroup.alpha   = n.InOutQuadratic();
			                                      unavailableGroup.alpha = (1.0f - n).InOutQuadratic();
		                                      };

		tile.available.onInactiveNormalFrame += n =>
		                                        {
			                                        availableGroup.alpha   = n.InOutQuadratic();
			                                        unavailableGroup.alpha = (1.0f - n).InOutQuadratic();
		                                        };

//		tile.experience.onContentAvailabilityChange += a =>
//		                                               {
////			                                               Debug.Log($"[{tile.experience.title}] is now [{(a ? "available" : "unavailable")}]");
//			                                               
//			                                               contentAvailable.active = a;
//		                                               };

//		tile.experience.UpdateContentAvailability();
	}

}