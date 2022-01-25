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

	public RectTransform backgroundRect;

	public Image background;

	public float hoverZoomScale = 1.1f;
	
	public float fadeEffect     = 0.75f;

//	public float c;
	
//	public Color from;
//	public Color to;
	
	void Awake()
	{
		if (!tile) tile = GetComponentInParent<Tile>();

		tile.onSelected += () =>
		                   {
			                   background.raycastTarget = false;
		                   };
		
		tile.onDeselected += () =>
		                   {
			                   background.raycastTarget = true;
		                   };

		tile.onHoverNormalFrame += n =>
		                           {
			                           n = n.InOutQuadratic();

			                           var m = Mathf.Lerp(a: 1.0f,
			                                              b: hoverZoomScale,
			                                              t: n);

			                           backgroundRect.localScale = m * Vector3.one;

			                           // Stop here if the tile is selected and no longer hovered

			                           if (tile.selected &&
			                               !tile.hovered) return;


			                           background.color = Color.Lerp(a: Color.black,
			                                                         b: Color.white,
			                                                         t: Mathf.Lerp(a: 1.0f,
			                                                                       b: fadeEffect,
			                                                                       t: n));
		                           };
		
		tile.onSelectNormalFrame += n =>
		                            {
			                            // Only proceed if the tile is becoming selected
			                            
			                            if (!tile.selected) return;
			                            
			                            background.color = Color.Lerp(a: Color.black,
			                                                          b: Color.white,
			                                                          t: Mathf.Lerp(a: 1.0f,
			                                                                        b: fadeEffect,
			                                                                        t: n));
		                            };
	}


	public void OnPointerEnter(PointerEventData eventData) => tile.HoverStart();

	public void OnPointerExit(PointerEventData eventData) => tile.HoverEnd();

	public void OnPointerClick(PointerEventData eventData) => tile.TrySelect();

}