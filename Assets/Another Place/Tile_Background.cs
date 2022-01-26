using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Vanilla.Easing;

public class Tile_Background : Tile_Element
{

	public RectTransform rect;

	public Image background;

	public float hoverZoomScale = 1.1f;

	public static Color normalColor = Color.white;
	public static Color hoverColor = new Color(r: 0.75f,
	                                           g: 0.75f,
	                                           b: 0.75f,
	                                           a: 1.0f);


	public override void Awake()
	{
		base.Awake();

		tile.onPopulate += HandlePopulate;

		tile.onFocusInFrame += ScaleBackground;
		tile.onFocusInFrame += TintBackground;

		tile.onFocusOutFrame += ScaleBackground;
		tile.onFocusOutFrame += TintBackground;
	}


	private void HandlePopulate(Experience e)
	{
		var   s = e.sprite;
		float w = s.texture.width;
		float h = s.texture.height;

		if (w < Tile.maxWindowSize) w            = Tile.maxWindowSize;
		if (h < Tile.minBackgroundImageHeight) h = Tile.minBackgroundImageHeight;

		rect.sizeDelta = new Vector2(x: w,
		                             y: h);

		background.sprite = s;
	}


	private void ScaleBackground(float n) => rect.localScale = Mathf.Lerp(a: 1.0f,
	                                                                      b: hoverZoomScale,
	                                                                      t: n.InOutQuadratic()) *
	                                                           Vector3.one;


	private void TintBackground(float n) => background.color = Color.Lerp(a: normalColor,
	                                                                      b: hoverColor,
	                                                                      t: n.InOutQuadratic());

}