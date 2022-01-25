using UnityEngine;
using UnityEngine.UI;

using Vanilla.Easing;

public class Tile_Preview : MonoBehaviour
{


	public Tile tile;

	public RectTransform backgroundRect;

	public Image background;

	public CanvasGroup previewGroup;

	
	public float hoverZoomScale = 1.1f;

	public float hoverFadeEffect = 0.75f;


	void Awake()
	{
		if (!tile) tile = GetComponentInParent<Tile>();

		tile.onHoverNormalFrame += n =>
		                           {
			                           n = n.InOutQuadratic();

			                           // Create a separate limited lerp value just for changing background image color

//			                           background.color = Color.Lerp(a: Color.black,
//			                                                         b: Color.white,
//			                                                         t: Mathf.Lerp(a: 1.0f,
//			                                                                       b: hoverFadeEffect,
//			                                                                       t: n));

			                           // Zoom background image scale

//			                           var m = Mathf.Lerp(a: 1.0f,
//			                                              b: hoverZoomScale,
//			                                              t: n);
//
//			                           backgroundRect.localScale = m * Vector3.one;

			                           // Fade preview canvas group alpha

			                           previewGroup.alpha = n.InOutQuadratic();
		                           };
	}

}