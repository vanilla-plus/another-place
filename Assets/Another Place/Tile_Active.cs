using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vanilla.Easing;

public class Tile_Active : MonoBehaviour
{

	public CanvasGroup activeGroup;

//	public RectTransform headerRect;

//	public float headerMinHeight = 0.0f;
//	public float headerMaxHeight = 320.0f;
	
	void Awake()
	{
		var tile = GetComponentInParent<Tile>();

		tile.onBecameSelected += () => activeGroup.gameObject.SetActive(true);
		
		tile.onSelectNormalFrame += n =>
		                            {
			                            activeGroup.alpha = n.InOutQuadratic();

			                            if (n < 0.001f)
			                            {
				                            activeGroup.gameObject.SetActive(false);
			                            }
		                            };
	}

}
