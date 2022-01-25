using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vanilla.Easing;

public class Tile_Active : MonoBehaviour
{

	public CanvasGroup activeGroup;
	
	void Awake() => GetComponentInParent<Tile>().onSelectNormalFrame += n => activeGroup.alpha = n.InOutQuadratic();

}
