using System;
using System.Threading.Tasks;

using UnityEngine;

using static UnityEngine.Debug;

public class Tile : MonoBehaviour
{

	public static Tile Selected;

	[Header("References")]
	public Tile_Layout_Flex_Horizontal layout;

	public RectTransform _rect;

	public Experience experience;

	[Header("Hover state")]
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
			{
				onHover?.Invoke();

				FillHoverNormal();
			}
			else
			{
				onDehover?.Invoke();

				DrainHoverNormal();
			}
		}
	}

	[SerializeField]
	private float _hoverNormal;
	public float hoverNormal
	{
		get => _hoverNormal;
		set => _hoverNormal = Mathf.Clamp01(value);
	}

	public float hoverNormalRate = 2.0f;

	[Header("Selected state")]
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

				onSelect?.Invoke();

				FillSelectNormal();
			}
			else
			{
				onDeselect?.Invoke();

				DrainSelectNormal();
			}
		}
	}

	[SerializeField]
	private float _selectNormal = 0.0f;
	public float selectNormal
	{
		get => _selectNormal;
		set => _selectNormal = Mathf.Clamp01(value);
	}

	public float selectNormalRate = 2.0f;

	[Header("Focus state")]
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
			{
				onFocus?.Invoke();

				FillFocusNormal();
			}
			else
			{
				onDefocus?.Invoke();

				DrainFocusNormal();
			}
		}
	}

	[SerializeField]
	private float _focusNormal;
	public float focusNormal
	{
		get => _focusNormal;
		set => _focusNormal = Mathf.Clamp01(value);
	}

	public float focusNormalRate = 2.0f;

	// Static Actions

	public static Action<Tile, Tile> OnSelectedChanged;

	// Data Actions

	[Tooltip("Invoked when the tile receives its Experience data payload class.")]
	public Action<Experience> onPopulate;

	// Hover Actions

	[Tooltip("Invoked whenever the tile is hovered over by a pointer, regardless of hoverNormal.")]
	public Action onHover;

	[Tooltip("Invoked whenever the tile is hovered over by a pointer & hoverNormal is 0. hoverNormal will begin accumulating delta time until it reaches 1.")]
	public Action onHoverNormalStart;

	[Tooltip("Invoked every frame hoverNormal is increased.")]
	public Action<float> onHoverNormalFrame;

	[Tooltip("Invoked when hoverNormal has reached its maximum value of 1.")]
	public Action onHoverNormalComplete;

	// Dehover Actions

	[Tooltip("Invoked when the tile is dehovered, regardless of hoverNormal.")]
	public Action onDehover;

	[Tooltip("Invoked when the tile is dehovered & hoverNormal is 1. hoverNormal will begin decumulating delta time until it reaches 0.")]
	public Action onDehoverNormalStart;

	[Tooltip("Invoked every frame hoverNormal is decreased.")]
	public Action<float> onDehoverNormalFrame;

	[Tooltip("Invoked when hoverNormal has reached its minimum value of 0.")]
	public Action onDehoverNormalComplete;

	// Select Actions

	[Tooltip("Invoked when the tile is selected, regardless of selectNormal.")]
	public Action onSelect;

	[Tooltip("Invoked when the tile is selected & selectNormal is 0. SelectNormal will begin accumulating delta time until it reaches 1.")]
	public Action onSelectNormalStart;

	[Tooltip("Invoked every frame selectNormal is increased.")]
	public Action<float> onSelectNormalFrame;

	[Tooltip("Invoked when selectNormal has reached its maximum value of 1.")]
	public Action onSelectNormalComplete;

	// Deselect Actions

	[Tooltip("Invoked when the tile is deselected, regardless of selectNormal.")]
	public Action onDeselect;

	[Tooltip("Invoked when a different tile has been selected & selectNormal is 1. SelectNormal will begin decumulating delta time until it reaches 0.")]
	public Action onDeselectNormalStart;

	[Tooltip("Invoked every frame selectNormal is decreased.")]
	public Action<float> onDeselectNormalFrame;

	[Tooltip("Invoked when selectNormal has reached its minimum value of 0.")]
	public Action onDeselectNormalComplete;

	// Focus Actions

	[Tooltip("Invoked when the tile is focused (hovered or selected), regardless of focusNormal.")]
	public Action onFocus;

	[Tooltip("Invoked when the tile is focused (hovered or selected) & focusNormal is 0. FocusNormal will begin accumulating delta time until it reaches 1.")]
	public Action onFocusNormalStart;

	[Tooltip("Invoked every frame focusNormal is increased.")]
	public Action<float> onFocusNormalFrame;

	[Tooltip("Invoked when focusNormal has reached its maximum value of 1.")]
	public Action onFocusNormalComplete;

	// Defocus Actions

	[Tooltip("Invoked when the tile is no longer focused (neither hovered or selected), regardless of focusNormal.")]
	public Action onDefocus;

	[Tooltip("Invoked when the tile is no longer focused (neither hovered or selected) & focusNormal is 1. FocusNormal will begin decumulating delta time until it reaches 0.")]
	public Action onDefocusNormalStart;

	[Tooltip("Invoked every frame focusNormal is decreased.")]
	public Action<float> onDefocusNormalFrame;

	[Tooltip("Invoked when focusNormal has reached its minimum value of 0.")]
	public Action onDefocusNormalComplete;

	[Header("Window Size")]
	public float minWindowSize = 700.0f;
	public float maxWindowSize = 1920.0f;

//	public float minBackgroundImageWidth  = 1920.0f;
//	public float minBackgroundImageHeight = 1280.0f;


	public void Awake()
	{
		if (!_rect) _rect = (RectTransform)transform;
	}


	public async Task Populate(Experience e)
	{
		gameObject.name = e.title;

		experience = e;

		onPopulate?.Invoke(e);
	}


	internal static bool NothingSelected =>
		ReferenceEquals(objA: Selected,
		                objB: null);


	internal void Hover()
	{
		hovered = true;

		focused = true;
	}


	internal void Dehover()
	{
		hovered = false;

		focused = selected; // The tile may still be focused i.e. it is selected.
	}


	public void Select()
	{
		selected = true;

		focused = true;
	}


	public void Deselect()
	{
		selected = false;

		focused = hovered; // The tile may still be focused i.e. if a pointer is hovering over it.
	}


	private async Task FillHoverNormal()
	{
		if (Mathf.Approximately(a: hoverNormal,
		                        b: 0.0f)) onHoverNormalStart?.Invoke();

		while (hovered && hoverNormal < 1.0f)
		{
			hoverNormal += Time.deltaTime * hoverNormalRate;

			onHoverNormalFrame?.Invoke(hoverNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: hoverNormal,
		                        b: 1.0f)) onHoverNormalComplete?.Invoke();
	}


	private async Task DrainHoverNormal()
	{
		if (Mathf.Approximately(a: hoverNormal,
		                        b: 1.0f)) onDehoverNormalStart?.Invoke();

		while (!hovered &&
		       hoverNormal > 0.0f)
		{
			hoverNormal -= Time.deltaTime * hoverNormalRate;

			onDehoverNormalFrame?.Invoke(hoverNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: hoverNormal,
		                        b: 0.0f)) onDehoverNormalComplete?.Invoke();
	}


	private async Task FillFocusNormal()
	{
		if (Mathf.Approximately(a: focusNormal,
		                        b: 0.0f)) onFocusNormalStart?.Invoke();

		while (focused && focusNormal < 1.0f)
		{
			focusNormal += Time.deltaTime * focusNormalRate;

			onFocusNormalFrame?.Invoke(focusNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: focusNormal,
		                        b: 1.0f)) onFocusNormalComplete?.Invoke();
	}


	private async Task DrainFocusNormal()
	{
		if (Mathf.Approximately(a: focusNormal,
		                        b: 1.0f)) onDefocusNormalStart?.Invoke();

		while (!focused &&
		       focusNormal > 0.0f)
		{
			focusNormal -= Time.deltaTime * focusNormalRate;

			onDefocusNormalFrame?.Invoke(focusNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: focusNormal,
		                        b: 0.0f)) onDefocusNormalComplete?.Invoke();
	}


	private async Task FillSelectNormal()
	{
		if (Mathf.Approximately(a: selectNormal,
		                        b: 0.0f)) onSelectNormalStart?.Invoke();

		while (selected &&
		       selectNormal < 1.0f)
		{
			selectNormal += Time.deltaTime * selectNormalRate;

			onSelectNormalFrame?.Invoke(selectNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: selectNormal,
		                        b: 1.0f)) onSelectNormalComplete?.Invoke();
	}


	private async Task DrainSelectNormal()
	{
		if (Mathf.Approximately(a: selectNormal,
		                        b: 1.0f)) onDeselectNormalStart?.Invoke();

		while (!selected &&
		       selectNormal > 0.0f)
		{
			selectNormal -= Time.deltaTime * selectNormalRate;

			onDeselectNormalFrame?.Invoke(selectNormal);

			await Task.Yield();
		}

		if (Mathf.Approximately(a: selectNormal,
		                        b: 0.0f)) onDeselectNormalComplete?.Invoke();
	}

}