using System;
using System.Threading.Tasks;

using UnityEngine;

public class Tile : MonoBehaviour
{

	public static Tile               Selected;
	public static Action<Tile, Tile> OnSelectedChanged;

	public Tile_Layout_Flex_Horizontal layout;

	public Experience experience;

//	public ILayout<Tile, Vector2> window;
//	public Tile       prev;

	public RectTransform _rect;

	[SerializeField]
	private bool _hovered = false;
	public bool hovered
	{
		get => _hovered;
		set
		{
			if (_hovered == value) return;

			_hovered = value;

			if (_hovered) 
				HoverIn();
			else 
				HoverOut();
		}
	}
	
	[SerializeField]
	private bool _selected = false;
	public bool selected
	{
		get => _selected;
		set
		{
			if (_selected == value) return;

			_selected = value;

			if (_selected)
			{
				if (!NothingSelected) Selected.Deselect();
		
				var oldSelected = Selected;

				Selected = this;

				OnSelectedChanged?.Invoke(arg1: oldSelected,
				                          arg2: this);
				
				SelectIn();
			}
			else
			{
				SelectOut();
			}
		}
	}
	
	[SerializeField]
	private bool _focused = false;
	public bool focused
	{
		get => _focused;
		set
		{
			if (_focused == value) return;

			_focused = value;

			if (_focused) 
				FocusIn();
			else 
				FocusOut();
		}
	}
	
	[SerializeField]
	private float _hoverNormal;
	public float hoverNormal
	{
		get => _hoverNormal;
		set
		{
			_hoverNormal = Mathf.Clamp01(value);

//			onHoverNormalChanged?.Invoke(_hoverNormal);
		}
	}
	
	[SerializeField]
	private float _selectNormal = 0.0f;
	public float selectNormal
	{
		get => _selectNormal;
		set
		{
			_selectNormal = Mathf.Clamp01(value);

//			onSelectNormalChanged?.Invoke(_selectNormal);
		}
	}

	[SerializeField]
	private float _focusNormal;
	public float focusNormal
	{
		get => _focusNormal;
		set
		{
			_focusNormal = Mathf.Clamp01(value);

//			onFocusNormalChanged?.Invoke(_focusNormal);
		}
	}
	
	public float hoverNormalRate  = 2.0f;
	public float selectNormalRate = 2.0f;
	public float focusNormalRate  = 2.0f;

	[Tooltip("Invoked when the tile receives its Experience data payload class.")]
	public Action<Experience> onPopulate;

	// Hover Actions
	
	[Tooltip("Invoked when a pointer enters the tile. The hover normal will begin accumulating delta time until it reaches 1.0.")]
	public Action onHoverInStart;

	[Tooltip("Invoked every frame the hover normal is increased.")]
	public Action<float> onHoverInFrame;

	[Tooltip("Invoked when the hover normal has reached its maximum value of 1.0.")]
	public Action onHoverInEnd;

	[Tooltip("Invoked when a pointer exits the tile. The hover normal will begin decumulating delta time until it reaches 0.0.")]
	public Action onHoverOutStart;
	
	[Tooltip("Invoked every frame the hover normal is decreased.")]
	public Action<float> onHoverOutFrame;

	[Tooltip("Invoked when the hover normal has reached its minimum value of 0.0.")]
	public Action onHoverOutEnd;
	
	// Select Actions
	
	[Tooltip("Invoked when a pointer clicks the tile. The select normal will begin accumulating delta time until it reaches 1.0.")]
	public Action onSelectInStart;
	
	[Tooltip("Invoked every frame the select normal is increased.")]
	public Action<float> onSelectInFrame;
	
	[Tooltip("Invoked when the select normal has reached its maximum value of 1.0.")]
	public Action onSelectInEnd;

	[Tooltip("Invoked when a different tile has been selected. The select normal will begin decumulating delta time until it reaches 0.0.")]
	public Action onSelectOutStart;
	
	[Tooltip("Invoked every frame the select normal is decreased.")]
	public Action<float> onSelectOutFrame;
	
	[Tooltip("Invoked when the select normal has reached its minimum value of 0.0.")]
	public Action onSelectOutEnd;
	
	// Focus Actions
	
	[Tooltip("Invoked when the tile is focused (hovered or selected), whichever happens first. The focus normal will begin accumulating delta time until it reaches 1.0.")]
	public Action onFocusInStart;
	
	[Tooltip("Invoked every frame the focus normal is increased.")]
	public Action<float> onFocusInFrame;
	
	[Tooltip("Invoked when the focus normal has reached its maximum value of 1.0.")]
	public Action onFocusInEnd;

	[Tooltip("Invoked when the tile is no longer focused (hovered or selected). The focus normal will begin decumulating delta time until it reaches 0.0.")]
	public Action onFocusOutStart;
	
	[Tooltip("Invoked every frame the focus normal is decreased.")]
	public Action<float> onFocusOutFrame;
	
	[Tooltip("Invoked when the focus normal has reached its minimum value of 0.0.")]
	public Action onFocusOutEnd;
	
//	public Action<float> onHoverNormalChanged;
//	public Action<float> onSelectNormalChanged;
//	public Action<float> onFocusNormalChanged;

	//	public Action<float> onDehoverNormalFrame;
//	public Action<float> onDeselectNormalFrame;

	public const float minWindowSize = 700.0f;
	public const float maxWindowSize = 1920.0f;

	public const float minBackgroundImageHeight = 1280.0f;

	public const float margin = 25.0f;

	public float xPosition;

//	private const float expandDuration = 0.1666f;

//	private float expandVel;

//	public float expandTimer = 0.0f;


	public void Awake()
	{
		if (!_rect) _rect = (RectTransform)transform;
	}


//	[ContextMenu("Click")]
//	public void OnClick() => Menu.i.currentTile = this;


//	public float GetRelativePosition() => _rect.anchoredPosition.x + _rect.sizeDelta.x * 0.5f + Menu.i.margin;
//	public float GetHalfWidth() => _rect.sizeDelta.x * 0.5f + margin;
//
//
//	public void Arrange()
//	{
//		if (!prev) return;
//
//		// My x position is the x position of prev plus half of its width and half of my width (and some margin x 2)
//
//		xPosition = prev.xPosition + prev.GetHalfWidth() + GetHalfWidth();
//
//		_rect.anchoredPosition = new Vector2(x: xPosition,
//		                                     y: 0.0f);
//	}


	public async Task Populate(Experience e)
	{
		gameObject.name = e.title;

		experience = e;
		
		onPopulate?.Invoke(e);
	}


//	public bool IsFocused => hovered || IsSelected;
//
//	public float FocusNormal => Mathf.Max(a: hoverNormal,
//	                                      b: selectNormal);

	internal static bool NothingSelected =>
		ReferenceEquals(objA: Selected,
		                objB: null);

	internal void StartHover()
	{
//		if (hovered) return;

		hovered = true;

//		HoverIn();

		focused = true;

//		FocusIn();
	}


	internal void EndHover()
	{
//		if (!hovered) return;

		hovered = false;
		
//		HoverOut();

		focused = selected; // The tile may still be focused i.e. it is selected.
		
//		if (!focused) FocusOut();
	}


	public void Select()
	{
		// Can't be Selected twice!

//		if (selected) return;
		
		selected = true;

		focused = true;

		// Tell the old guy he's fired, if he exists

//		if (!NothingSelected) Selected.Deselect();

//		var oldSelected = Selected;

//		Selected = this;

//		onSelect?.Invoke();

//		OnSelectedChanged?.Invoke(arg1: oldSelected,
//		                          arg2: this);

//		SelectIn();
	}


	public void Deselect()
	{
//		onDeselect?.Invoke();

		selected = false;
		
//		SelectOut();

		focused  = hovered; // The tile may still be focused i.e. if a pointer is hovering over it.

//		ShrinkSizeHandler();

//		if (!focused) FocusOut();
	}


	private async Task HoverIn()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onHoverInStart?.Invoke();

		if (Mathf.Approximately(a: hoverNormal,
		                        b: 0.0f)) onHoverInStart?.Invoke();

		while (hovered && hoverNormal < 1.0f)
		{
			hoverNormal += Time.deltaTime * hoverNormalRate;

			onHoverInFrame?.Invoke(hoverNormal);
			
			await Task.Yield();
		}

		if (Mathf.Approximately(a: hoverNormal,
		                        b: 1.0f)) onHoverInEnd?.Invoke();
	}


	private async Task HoverOut()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onHoverOutStart?.Invoke();

		if (Mathf.Approximately(a: hoverNormal,
		                        b: 1.0f)) onHoverOutStart?.Invoke();
		
		while (!hovered && hoverNormal > 0.0f)
		{
			hoverNormal -= Time.deltaTime * hoverNormalRate;

			onHoverOutFrame?.Invoke(hoverNormal);

			await Task.Yield();
		}
		
		if (Mathf.Approximately(a: hoverNormal,
		                        b: 0.0f)) onHoverOutEnd?.Invoke();
	}
	
	private async Task FocusIn()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onFocusInStart?.Invoke();

		if (Mathf.Approximately(a: focusNormal,
		                        b: 0.0f)) onFocusInStart?.Invoke();

		while (focused && focusNormal < 1.0f)
		{
			focusNormal += Time.deltaTime * focusNormalRate;

			onFocusInFrame?.Invoke(focusNormal);
			
			await Task.Yield();
		}
		
		if (Mathf.Approximately(a: focusNormal,
		                        b: 1.0f)) onFocusInEnd?.Invoke();
	}


	private async Task FocusOut()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onFocusOutStart?.Invoke();

		if (Mathf.Approximately(a: focusNormal,
		                        b: 1.0f)) onFocusOutStart?.Invoke();

		while (!focused &&
		       focusNormal > 0.0f)
		{
			focusNormal -= Time.deltaTime * focusNormalRate;

			onFocusOutFrame?.Invoke(focusNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: focusNormal,
		                        b: 0.0f)) onFocusOutEnd?.Invoke();
	}


	private async Task SelectIn()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onSelectInStart?.Invoke();

		if (Mathf.Approximately(a: selectNormal,
		                        b: 0.0f)) onSelectInStart?.Invoke();

		while (selected &&
		       selectNormal < 1.0f)
		{
			selectNormal += Time.deltaTime * selectNormalRate;

			onSelectInFrame?.Invoke(selectNormal);

			await Task.Yield();
		}
		
		if (Mathf.Approximately(a: selectNormal,
		                        b: 1.0f)) onSelectInEnd?.Invoke();
	}


	private async Task SelectOut()
	{
		// Only invoke this event if the normal is truly starting from the start or end
		// Keeping the naive approach just in case this check causes unexpected behaviour in practise.
		
		// onSelectOutStart?.Invoke();

		if (Mathf.Approximately(a: selectNormal,
		                        b: 1.0f)) onSelectOutStart?.Invoke();
		
		while (!selected && selectNormal > 0.0f)
		{
			selectNormal -= Time.deltaTime * selectNormalRate;

			onSelectOutFrame?.Invoke(selectNormal);

			await Task.Yield();
		}
		
		if (Mathf.Approximately(a: selectNormal,
		                        b: 0.0f)) onSelectOutEnd?.Invoke();
	}

//
//	private async Task ExpandSizeHandler()
//	{
//		while (IsSelected && Mathf.Abs(_rect.sizeDelta.x - maxWindowSize) > 0.1f)
//		{
//			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
//			                                                  target: maxWindowSize,
//			                                                  currentVelocity: ref expandVel,
//			                                                  smoothTime: expandDuration),
//			                              y: _rect.sizeDelta.y);
//
//			await Task.Yield();
//		}
//	}
//
//
//	private async Task ShrinkSizeHandler()
//	{
//		while (!IsSelected &&
//		       Mathf.Abs(_rect.sizeDelta.x - minWindowSize) > 0.1f)
//		{
//			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
//			                                                  target: minWindowSize,
//			                                                  currentVelocity: ref expandVel,
//			                                                  smoothTime: expandDuration),
//			                              y: _rect.sizeDelta.y);
//
//			await Task.Yield();
//		}
//	}

}