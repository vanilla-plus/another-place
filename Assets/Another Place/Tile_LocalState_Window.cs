using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Vanilla.Easing;

public class Tile_LocalState_Window : Tile_Element
{

	public RectTransform rect;

	public float unavailableSize = 220.0f;
	public float downloadingSize = 250.0f;
	public float readySize       = 330.0f;

	public float minSize    = 100.0f;
	public float targetSize = 100.0f;

	public void Start()
	{
		tile.select.onActiveNormalFrame   += AnimateSize;
		tile.select.onInactiveNormalFrame += AnimateSize;

		tile.experience.onDownloadBegun    += () => UpdateTargetSize(downloadingSize);
		tile.experience.onDownloadComplete += () => UpdateTargetSize(readySize);
		
		tile.experience.onContentAvailabilityChange += available => UpdateTargetSize(available ? readySize : unavailableSize);
	}
	
	public void UpdateTargetSize(float newSize)
	{
		targetSize = newSize;

		if (tile.select.fullyActive) SetSizeImmediately();
	}


	public void AnimateSize(float n) => rect.sizeDelta = new Vector2(x: Mathf.Lerp(a: minSize,
	                                                                           b: targetSize,
	                                                                           t: n.InOutQuadratic()),
	                                                             y: rect.sizeDelta.y);


	public void SetSizeImmediately() => rect.sizeDelta = new Vector2(x: targetSize,
	                                                      y: rect.sizeDelta.y);

}