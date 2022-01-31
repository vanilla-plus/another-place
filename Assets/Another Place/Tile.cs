using System;
using System.Threading.Tasks;

using SimpleJSON;

using UnityEngine;

using static UnityEngine.Debug;

public class Tile : MonoBehaviour
{

	public static Tile Selected;

	[Header("References")]
	public Tile_Layout_Flex_Horizontal layout;

	public RectTransform _rect;

	public Experience experience;

//	public bool requiresArranging = false;
	
	[Header("Hover state")]
	public Tile_State hover = new Tile_State();

	[Header("Select state")]
	public Tile_State select = new Tile_State();

	[Header("Focus state")]
	public Tile_State focus = new Tile_State();

	[Header("Download Request State")]
	public Tile_State download_request = new Tile_State();
	
	[Header("Downloading State")]
	public Tile_State downloading = new Tile_State();
	
	// Static Actions

	public static Action<Tile, Tile> OnSelectedChanged;

	// Data Actions

	[Tooltip("Invoked when the tile receives its Experience data payload class.")]
	public Action<Experience, JSONNode> onPopulate;
	
	[Header("Window Size")]
	public float minWindowSize = 700.0f;
	public float maxWindowSize = 1920.0f;

	public void Awake()
	{
		if (!_rect) _rect = (RectTransform)transform;

		select.onActive += HandleBecomingSelected;
	}

	private void HandleBecomingSelected()
	{
		if (!NothingSelected) Selected.Deselect();

		var oldSelected = Selected;

		Selected = this;

		OnSelectedChanged?.Invoke(arg1: oldSelected,
									arg2: this);
	}


	public async Task Populate(Experience e, JSONNode page)
	{
		gameObject.name = e.title;

		experience = e;

		onPopulate?.Invoke(arg1: e,
		                   arg2: page);
	}


	internal static bool NothingSelected =>
		ReferenceEquals(objA: Selected,
		                objB: null);


	internal void Hover()
	{
		hover.active = true;

		focus.active = true;
	}


	internal void Dehover()
	{
		hover.active = false;

		focus.active = select.active; // The tile may still be focused i.e. it is selected.
	}


	public void Select()
	{
		select.active = true;

		focus.active = true;
	}


	public void Deselect()
	{
		select.active = false;

		focus.active = hover.active; // The tile may still be focused i.e. if a pointer is hovering over it.
	}
	
}