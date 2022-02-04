using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vanilla.Easing;

public class Tile_Group : Tile_Element
{

	public CanvasGroup group;

	public Tile_State state;

	public void OnEnable()
	{
		state.onActiveNormalStart      += () => group.gameObject.SetActive(true);
		state.onInactiveNormalComplete += () => group.gameObject.SetActive(false);

		state.onActiveNormalFrame += n => group.alpha = n.InOutQuadratic();

		state.onInactiveNormalFrame += n => group.alpha = n.InOutQuadratic();
	}

}