using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Vanilla.Easing;

public class Tile_Background : MonoBehaviour
{

	public Tile tile;

	public RectTransform backgroundRect;

	public Image background;

	public float hoverZoomScale = 1.1f;

	public static Color normalColor = Color.white;
	public static Color hoverColor = new Color(r: 0.75f,
	                                           g: 0.75f,
	                                           b: 0.75f,
	                                           a: 1.0f);


	void Awake()
	{
		
		
//		tile.onBecameSelected += () => background.raycastTarget = false;
//
//		tile.onBecameSelected += () => background.raycastTarget = true;

		tile.onHoverNormalFrame += HandleHoverFrame;

		tile.onDehoverNormalFrame += HandleDehoverFrame;

		tile.onDeselectNormalFrame += HandleHoverFrame;

//		tile.onSelectNormalFrame += HandleHoverFrame;
//
//		tile.onDeselectNormalFrame += HandleHoverFrame;

//		tile.onSelectNormalFrame += n =>
//		                            {
//			                            // Only proceed if the tile is becoming selected
//
//			                            if (!tile.IsSelected) return;
//
//			                            background.color = Color.Lerp(a: Color.black,
//			                                                          b: Color.white,
//			                                                          t: Mathf.Lerp(a: 1.0f,
//			                                                                        b: fadeEffect,
//			                                                                        t: n));
//		                            };
	}


//	public void Update()
//	{
//		if (tile.hovered || tile.IsSelected)
//		{
//			
//		}
//		else
//		{
//			
//		}
//	}

	

	private void HandleHoverFrame(float n)
	{
		n = tile.hoverNormal.InOutQuadratic();

		var z = Mathf.Lerp(a: 1.0f,
		                   b: hoverZoomScale,
		                   t: n);

		backgroundRect.localScale = z * Vector3.one;

		// Stop here if the tile is selected and no longer hovered

//		if (tile.IsSelected &&
//		    !tile.hovered) return;

		background.color = Color.Lerp(a: normalColor,
		                              b: hoverColor,
		                              t: n);
	}

//
	private void HandleDehoverFrame(float n)
	{
		if (tile.IsSelected) return;

		HandleHoverFrame(n);
	}

}