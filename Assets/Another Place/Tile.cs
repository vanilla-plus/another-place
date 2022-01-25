using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UnityEngine.Debug;

public class Tile : MonoBehaviour
{

	public static Tile               Selected;
	public static Action<Tile, Tile> OnSelect;
	
	public Tile prev;
	
	public RectTransform _rect;

	public Button _button; 
//	public Image _image;

	public RectTransform _backgroundRect;
	public Image         _backgroundImage;

	public TMP_Text hoverHeading;

	public bool  hovered     = false;

	[SerializeField]
	private float _hoverNormal    = 0.0f;
	public float hoverNormal
	{
		get => _hoverNormal;
		set
		{
			_hoverNormal = value;
			
			onHoverNormalFrame?.Invoke(value);
		}
	}

	public  float hoverNormalRate = 1.0f;

	public Action<float> onHoverNormalFrame;

	public bool  selected         = false;
	
	[SerializeField]
	private float _selectNormal = 0.0f;
	public float selectNormal
	{
		get => _selectNormal;
		set
		{
			_selectNormal = value;
			
			onSelectNormalFrame?.Invoke(value);
		}
	}
	
	public float selectNormalRate = 1.0f;

	public Action<float> onSelectNormalFrame;

	public const float minWindowSize = 500.0f;
	public const float maxWindowSize = 1500.0f;

	public const float minBackgroundImageHeight = 1280.0f;

	public const float margin = 25.0f;

	public float xPosition;
	
	private const float expandDuration = 0.1666f;

	private float expandVel;

	public float expandTimer = 0.0f;

	public void Awake()
	{
		if (!_rect) _rect     = (RectTransform)transform;
		if (!_button) _button = GetComponentInChildren<Button>();

		OnSelect += (outgoing,
		             incoming) =>
		            {
			            if (outgoing == this && selected)
			            {
				            selected = false;
				            
				            DeSelectHandler();
			            }
		            };

		onSelectNormalFrame += n => { };
	}

//	[ContextMenu("Click")]
//	public void OnClick() => Menu.i.currentTile = this;


//	public float GetRelativePosition() => _rect.anchoredPosition.x + _rect.sizeDelta.x * 0.5f + Menu.i.margin;
	public float GetHalfWidth() => _rect.sizeDelta.x * 0.5f + margin;

	public void Arrange()
	{
		if (!prev) return;

		// My x position is the x position of prev plus half of its width and half of my width (and some margin x 2)
		
		xPosition = prev.xPosition + prev.GetHalfWidth() + GetHalfWidth();

		_rect.anchoredPosition = new Vector2(x: xPosition,
		                                     y: 0.0f);
	}

	public async Task Populate(Experience e)
	{
		// GameObject
		gameObject.name = e.title;
		
		// Background sprite
		
		var   s = e.sprite;
		float w = s.texture.width;
		float h = s.texture.height;

		if (w < maxWindowSize) w            = maxWindowSize;
		if (h < minBackgroundImageHeight) h = minBackgroundImageHeight;

		_backgroundRect.sizeDelta = new Vector2(x: w,
		                                        y: h);

		_backgroundImage.sprite = s;
		
		// Hover Heading

		hoverHeading.text = e.title;
	}


	public async Task HoverStart()
	{
		hovered = true;
		
		while (hovered && hoverNormal < 1.0f)
		{
			hoverNormal = Mathf.Clamp01(hoverNormal + Time.deltaTime * hoverNormalRate);
			
			await Task.Yield();
		}

		Log("HoverStart done");
	}
	
	public async Task HoverEnd()
	{
		hovered = false;

		while (!hovered && hoverNormal > 0.0f)
		{
			hoverNormal = Mathf.Clamp01(hoverNormal - Time.deltaTime * hoverNormalRate);

			await Task.Yield();
		}
		
		Log("HoverEnd done");
	}


	public void Select()
	{
		if (Selected == this) return;

		var oldSelected = Selected;
		
		Selected = this;

		selected = true;

		OnSelect?.Invoke(arg1: oldSelected,
		                 arg2: Selected);

		SelectHandler();
	}


	private async Task SelectHandler()
	{
		while (selected && Mathf.Abs(_rect.sizeDelta.x - maxWindowSize) > 0.1f)
		{
			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
			                                                  target: maxWindowSize,
			                                                  currentVelocity: ref expandVel,
			                                                  smoothTime: expandDuration),
			                              y: _rect.sizeDelta.y);
			
			await Task.Yield();
		}
		
//		while (selected &&
//		       selectNormal < 1.0f)
//		{
//			selectNormal = Mathf.Clamp01(selectNormal + Time.deltaTime * selectNormalRate);
//			
//			await Task.Yield();
//		}
	}


	private async Task DeSelectHandler()
	{
		while (!selected && Mathf.Abs(_rect.sizeDelta.x - minWindowSize) > 0.1f)
		{
			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
			                                                  target: minWindowSize,
			                                                  currentVelocity: ref expandVel,
			                                                  smoothTime: expandDuration),
			                              y: _rect.sizeDelta.y);
			
			await Task.Yield();
		}
		
//		while (!selected &&
//		       selectNormal > 0.0f)
//		{
//			selectNormal = Mathf.Clamp01(selectNormal - Time.deltaTime * selectNormalRate);
//
//			await Task.Yield();
//		}
	}


//	public void Update()
//	{
//
//		
//		if (selected)
//		{
//			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
//			                                                  target: maxWindowSize,
//			                                                  currentVelocity: ref expandVel,
//			                                                  smoothTime: expandDuration),
//			                              y: _rect.sizeDelta.y);
//
//			expandTimer += Time.deltaTime;
//
//			if (expandTimer > 1.0f) enabled = false;
//		}
//		else
//		{
//			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
//			                                                  target: minWindowSize,
//			                                                  currentVelocity: ref expandVel,
//			                                                  smoothTime: expandDuration),
//			                              y: _rect.sizeDelta.y);
//
//			expandTimer -= Time.deltaTime;
//
//			if (expandTimer < 0.0f) enabled = false;
//		}
//
//	}




}