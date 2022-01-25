using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vanilla.Easing;

public class Tile_Title : MonoBehaviour
{

	private float hoverNormal;
	private float selectNormal;
	
	private float totalAlpha;

	public float minHeight = 0.0f;
	public float maxHeight = 320.0f;

	public float scaleMin = 1.0f;
	public float scaleMax = 1.2f;
	
	private void Awake()
	{
		var rect = (RectTransform)transform;
		var tile = GetComponentInParent<Tile>();
		var c    = GetComponent<CanvasGroup>();

		tile.onSelectNormalFrame += n =>
		                            {
			                            selectNormal = n.InOutQuadratic();

			                            var h = Mathf.Lerp(a: minHeight,
			                                               b: maxHeight,
			                                               t: selectNormal);

			                            rect.anchoredPosition = new Vector2(x: 0,
			                                                                y: h);
			                            
			                            c.alpha = hoverNormal + selectNormal;

			                            rect.localScale = Vector3.one *
			                                              Mathf.Lerp(a: scaleMin,
			                                                         b: scaleMax,
			                                                         t: selectNormal);
		                            };

		tile.onHoverNormalFrame += n =>
		                           {
			                           hoverNormal = n.InOutQuadratic();

			                           c.alpha = hoverNormal + selectNormal;
		                           };
//		
//		tile.onSelectNormalFrame += n =>
//		                            {
//			                            
//			                            c.alpha = hoverNormal + selectNormal;
//		                            };
	}
	
}
