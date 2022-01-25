using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Vanilla.Easing;

public class Tile_Background : MonoBehaviour,
                               IPointerEnterHandler,
                               IPointerExitHandler,
                               IPointerClickHandler
{

	public Tile tile;

	public RectTransform rect;

	public Image background;
	
	public float hoverZoomScale = 1.1f;

	public float hoverFadeEffect = 0.75f;
	
	void Awake()
	{
		if (!tile) tile = GetComponentInParent<Tile>();

		tile.onHoverNormalFrame += n =>
		                           {
			                           n = n.InOutQuadratic();

			                           // Create a separate limited lerp value just for color

			                           background.color = Color.Lerp(a: Color.black,
			                                                         b: Color.white,
			                                                         t: Mathf.Lerp(a: 1.0f,
			                                                                       b: hoverFadeEffect,
			                                                                       t: n));

			                           var m = Mathf.Lerp(a: 1.0f,
			                                              b: hoverZoomScale,
			                                              t: n);

			                           rect.localScale = m * Vector3.one;
		                           };
	}


	public void OnPointerEnter(PointerEventData eventData) => tile.HoverStart();

	public void OnPointerExit(PointerEventData eventData) => tile.HoverEnd();

	public void OnPointerClick(PointerEventData eventData) => tile.Select();

//	void Update() => hoverNormal = Mathf.Clamp01(hoverNormal + (hovered ? Time.deltaTime * fadeInRate : -Time.deltaTime * fadeInRate));

}