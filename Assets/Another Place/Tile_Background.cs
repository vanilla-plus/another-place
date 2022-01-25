using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile_Background : MonoBehaviour,
                               IPointerEnterHandler,
                               IPointerExitHandler,
                               IPointerClickHandler
{

	public Tile tile;

	public Image background;

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
	}


	public void OnPointerEnter(PointerEventData eventData) => tile.HoverStart();

	public void OnPointerExit(PointerEventData eventData) => tile.HoverEnd();

	public void OnPointerClick(PointerEventData eventData) => tile.TrySelect();

}