using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	public Tile prev;
	
	public RectTransform _rect;

	public Button _button; 
//	public Image _image;

	public RectTransform _backgroundRect;
	public Image         _backgroundImage;

	public TMP_Text hoverHeading;
	
	public bool selected = false;

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
	}

	[ContextMenu("Click")]
	public void OnClick() => Menu.i.currentTile = this;


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


	public void Update()
	{
		if (selected)
		{
			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
			                                                  target: maxWindowSize,
			                                                  currentVelocity: ref expandVel,
			                                                  smoothTime: expandDuration),
			                              y: _rect.sizeDelta.y);

			expandTimer += Time.deltaTime;

			if (expandTimer > 1.0f) enabled = false;
		}
		else
		{
			_rect.sizeDelta = new Vector2(x: Mathf.SmoothDamp(current: _rect.sizeDelta.x,
			                                                  target: minWindowSize,
			                                                  currentVelocity: ref expandVel,
			                                                  smoothTime: expandDuration),
			                              y: _rect.sizeDelta.y);

			expandTimer -= Time.deltaTime;

			if (expandTimer < 0.0f) enabled = false;
		}

	}


	public void OnPointerEnter(PointerEventData eventData) => Debug.Log("Hover start?");

	public void OnPointerExit(PointerEventData eventData) => Debug.Log("Hover end?");

}