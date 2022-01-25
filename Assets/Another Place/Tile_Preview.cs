using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Vanilla.Easing;

public class Tile_Preview : MonoBehaviour
{

	void Awake()
	{
		var c = GetComponent<CanvasGroup>();
		
		GetComponentInParent<Tile>().onHoverNormalFrame += n => c.alpha = n.InOutQuadratic();
	}
	
//	public bool hovered = false;

//	public float alpha;
	
//	private const float fadeInRate = 2.0f;
	
//	public CanvasGroup canvasGroup;
	
//	public void OnPointerEnter(PointerEventData eventData)
//	{
//		Debug.Log("Hover start?");
//
//		hovered = true;
//	}


//	public void OnPointerExit(PointerEventData eventData)
//	{
//		Debug.Log("Hover end?");
//
//		hovered = false;
//	}


//	void Update()
//	{
//		alpha = Mathf.Clamp01(alpha + (hovered ? Time.deltaTime * fadeInRate : -Time.deltaTime * fadeInRate));
//
//		canvasGroup.alpha = alpha;
//	}

}