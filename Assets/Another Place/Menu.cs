using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

	public static Menu i;

	[SerializeField]
	private RectTransform _rect;

	private float _x;
	public float x
	{
		get => _x;
		set
		{
			_x = Mathf.Clamp(value,
			                 -tileSpace,
			                 -tileSpace * tiles.Count);
			
			
		}
	}

	public Vector2 dest;
	
	public List<Experience> experiences = new List<Experience>();

	public GameObject tilePrefab;

	public RectTransform tileParent;
	
	public List<Tile> tiles = new List<Tile>();

	public float tileSpace = -1;
	public float tileHalfSpace = -1;
	
	public float tilePixelWidth  = -1;
	public float tilePixelOffset = -1;

	public int tileCountLessOne = -1;
	
	private void Awake()
	{
		i = this;

		_rect = (RectTransform)tileParent.transform;

		dest = _rect.anchoredPosition;

		Place.onCatalogueFetched += Initialize;
	}


	public async void Initialize()
	{
		tilePixelOffset = GetComponentInChildren<HorizontalLayoutGroup>().spacing;
		
		experiences = Place.Catalogue;
		
		foreach (var e in experiences)
		{
			var newTile = Instantiate(original: tilePrefab,
			                          parent: tileParent,
			                          worldPositionStays: false).GetComponent<Tile>();

			if (tilePixelWidth < 0) tilePixelWidth = ((RectTransform)newTile.transform).sizeDelta.x;
			
			tiles.Add(newTile);
            
			await newTile.Populate(e);
		}

		tileSpace     = tilePixelWidth + tilePixelOffset;
		tileHalfSpace = tileSpace * 0.5f;

		tileCountLessOne = tiles.Count - 1;

		dest.x = tileCountLessOne * tileHalfSpace;
	}
	
	// SmoothStep testing

	// Minimum and maximum values for the transition.
	public float slideFrom = 10.0f;
	public float slideTo = 20.0f;

	// Time taken for the transition.
	public float slideDuration = 2.5f;

	public float startTime;

	public float slideTimer = -1.0f;

	void OnEnable()
	{
		ResetSlide();

	}


	public void ResetSlide()
	{
		startTime = Time.time;
		
		slideTimer = slideDuration;

	}

	void Update()
	{
		
		
		dest.x = Mathf.Clamp(dest.x,
		                     -tileSpace,
		                     -tileSpace * tiles.Count);
		
		_rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition,
		                                      dest,
		                                      Time.deltaTime);
	}

//	void Update()
//	{
////		var t = (Time.time - startTime) / duration;
//
//		dest.x = Mathf.SmoothStep(slideFrom,
//		                          slideTo,
//		                          Time.deltaTime);
//
//		_rect.anchoredPosition = dest;
//	}



}