using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Vanilla.Easing;

public class Tile_Button : Tile_Element, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

	public RectTransform rect;
	
	public Tile_State hover = new Tile_State();

	public float minScale = 1.0f;
	public float maxScale = 1.1f;

	public UnityEvent onClick;


	void Start()
	{
		hover.onActiveNormalFrame += n => rect.localScale = Vector3.one *
		                                                    Mathf.Lerp(a: minScale,
		                                                               b: maxScale,
		                                                               t: n.InOutQuadratic());

		hover.onInactiveNormalFrame += n => rect.localScale = Vector3.one *
		                                                      Mathf.Lerp(a: minScale,
		                                                                 b: maxScale,
		                                                                 t: n.InOutQuadratic());
	}
	
	public void OnPointerEnter(PointerEventData eventData) => hover.active = true;

	public void OnPointerExit(PointerEventData eventData) => hover.active = false;
	
	public void OnPointerClick(PointerEventData eventData) => onClick.Invoke();

	public void Honk() => Debug.Log("Honk honk!");

}
