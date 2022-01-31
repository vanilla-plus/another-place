using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile_Events : Tile_Element,
                           IPointerEnterHandler,
                           IPointerExitHandler,
                           IPointerClickHandler
{

	public Image background;


	public override void Awake()
	{
		base.Awake();

		tile.@select.onActive += () => background.raycastTarget = false;

		tile.@select.onInactive += () => background.raycastTarget = true;
	}


	public void OnPointerEnter(PointerEventData eventData) => tile.Hover();

	public void OnPointerExit(PointerEventData eventData) => tile.Dehover();

	public void OnPointerClick(PointerEventData eventData) => tile.Select();

}