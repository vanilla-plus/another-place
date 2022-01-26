using System.Collections;
using System.Collections.Generic;

using CurvedUI;

using TMPro;

using UnityEngine;

using Vanilla.Easing;

public class Tile_Title : Tile_Element
{

	public RectTransform rect;
	public CanvasGroup   group;

	public CurvedUITMP text;
	
	private float hoverNormal;
	private float selectNormal;

	private float totalAlpha;

	public float minHeight = 0.0f;
	public float maxHeight = 320.0f;

	public float scaleMin = 1.0f;
	public float scaleMax = 1.2f;


	public override void Awake()
	{
		base.Awake();

		if (!group) group = GetComponent<CanvasGroup>();
		if (!rect) rect   = (RectTransform)transform;

		var text = GetComponent<TextMeshProUGUI>();
		
//		var curvedText = GetComponent<CurvedUITMP>();

		tile.onPopulate += e =>
		                   {
			                   text.text        = e.title;
//			                   curvedText.Dirty = true;
		                   };
		
		tile.onFocusNormalStart  += () => group.gameObject.SetActive(true);
		tile.onFocusNormalFrame  += FadeText;
		tile.onDefocusNormalFrame += FadeText;
		tile.onDefocusNormalComplete   += () => group.gameObject.SetActive(false);

		tile.onSelectNormalFrame  += MoveText;
		tile.onDeselectNormalFrame += MoveText;

		tile.onSelectNormalFrame  += ScaleText;
		tile.onDeselectNormalFrame += ScaleText;

//		tile.onSelectNormalChanged += n =>
//		                            {
//			                            selectNormal = n.InOutQuadratic();
//
//			                            var h = Mathf.Lerp(a: minHeight,
//			                                               b: maxHeight,
//			                                               t: selectNormal);
//
//			                            rect.anchoredPosition = new Vector2(x: 0,
//			                                                                y: h);
//			                            
//			                            group.alpha = hoverNormal + selectNormal;
//
//			                            rect.localScale = Vector3.one *
//			                                              Mathf.Lerp(a: scaleMin,
//			                                                         b: scaleMax,
//			                                                         t: selectNormal);
//		                            };

//		tile.onFocusNormalChanged += n => c.alpha = n.InOutQuadratic();
//		
//		tile.onSelectNormalFrame += n =>
//		                            {
//			                            
//			                            c.alpha = hoverNormal + selectNormal;
//		                            };
	}


	private void FadeText(float n) => group.alpha = n.InOutQuadratic();


	private void MoveText(float n) => rect.anchoredPosition = new Vector2(x: 0,
	                                                                      y: Mathf.Lerp(a: minHeight,
	                                                                                    b: maxHeight,
	                                                                                    t: n.InOutQuadratic()));


	private void ScaleText(float n) => rect.localScale = Vector3.one *
	                                                     Mathf.Lerp(a: scaleMin,
	                                                                b: scaleMax,
	                                                                t: n.InOutQuadratic());

}