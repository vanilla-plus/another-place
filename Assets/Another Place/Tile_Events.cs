using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile_Events : MonoBehaviour,
                           IPointerEnterHandler,
                           IPointerExitHandler,
                           IPointerClickHandler
{

	public Tile tile;

	public Image background;


	void Awake()
	{
		tile.onBecameSelected += () =>
		                         {
			                         background.raycastTarget = false;
		                         };

		tile.onBecameDeselected += () =>
		                           {
			                           background.raycastTarget = true;
		                           };
	}
	
	public void OnPointerEnter(PointerEventData eventData) => tile.HoverStart();

	public void OnPointerExit(PointerEventData eventData) => tile.HoverEnd();

	public void OnPointerClick(PointerEventData eventData) => tile.TrySelect();

}