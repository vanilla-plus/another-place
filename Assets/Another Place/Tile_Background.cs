using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Vanilla.Easing;

public class Tile_Background : Tile_Element
{

	public RectTransform rect;

	public Image background;

	public bool enforceMinimumImageSize = true;

	public float minWidth = 1920.0f;
	public float minHeight = 1280.0f;
	
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

		tile.onFocusNormalFrame += ScaleBackground;
		tile.onFocusNormalFrame += TintBackground;

		tile.onDefocusNormalFrame += ScaleBackground;
		tile.onDefocusNormalFrame += TintBackground;
	}


	private void HandlePopulate(Experience e)
	{
		var s = e.sprite;

		var w = enforceMinimumImageSize ?
			        Mathf.Max(a: s.texture.width,
			                  b: minWidth) :
			        s.texture.width;

		var h = enforceMinimumImageSize ?
			        Mathf.Max(a: s.texture.height,
			                  b: minHeight) :
			        s.texture.height;

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