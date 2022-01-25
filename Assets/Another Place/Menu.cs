using System.Collections.Generic;

using UnityEngine;

public class Menu : MonoBehaviour
{

	public static Menu i;

	[SerializeField]
	private RectTransform _rect;

	private       float _x;
	private       float _xVel;
	private const float xAnimDuration = 0.1666f;
	
	public List<Experience> experiences = new List<Experience>();

	public GameObject tilePrefab;

	public RectTransform tileParent;

	[SerializeField]
	private Tile _currentTile = null;
	public Tile currentTile
	{
		get => _currentTile;
		set
		{
			if (_currentTile)
			{
				_currentTile.selected = false;
				_currentTile.enabled  = true;
			}

			if (value)
			{
				value.selected = true;
				value.enabled  = true;
			}

			_currentTile = value;
		}
	}

	public List<Tile> tiles = new List<Tile>();

	private void Awake()
	{
		i = this;

		_rect = (RectTransform)tileParent.transform;

		Place.onCatalogueFetched += Initialize;
	}


	public async void Initialize()
	{
		experiences = Place.Catalogue;

		Tile prev = null;

		foreach (var e in experiences)
		{
			var newTile = Instantiate(original: tilePrefab,
			                          parent: tileParent,
			                          worldPositionStays: false).GetComponent<Tile>();

			newTile.prev = prev;

			prev = newTile;
			
			tiles.Add(newTile);

			await newTile.Populate(e);
		}
		
		currentTile = tiles[0];

		enabled = true;
		
	}

	void Update()
	{
		foreach (var t in tiles) t.Arrange();

		if (!_currentTile) return;

		_x = Mathf.SmoothDamp(current: _x,
		                      target: -_currentTile.xPosition,
		                      currentVelocity: ref _xVel,
		                      smoothTime: xAnimDuration);

		_rect.anchoredPosition = new Vector2(x: _x,
		                                     y: 0);
	}

}