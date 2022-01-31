using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SimpleJSON;

using UnityEngine;

using static UnityEngine.Debug;

public class Menu : MonoBehaviour
{

	public static Menu i;

	[SerializeField]
	private RectTransform _rect;

	public        float _x;
	public        float _xVel;
	private const float xAnimDuration = 0.1666f;

	public List<Experience> experiences = new List<Experience>();

	public GameObject tilePrefab;

	public RectTransform tileParent;

	public bool arranging;
	
	public List<Tile> tiles = new List<Tile>();

	public Action onTransitionStart;
	
	private void Awake()
	{
		i = this;

		_x = 0.0f;

		Tile.OnSelectedChanged += TileSelectedHandler;
		
		_rect = (RectTransform)tileParent.transform;

		_rect.anchoredPosition = Vector2.zero;

		Place.onCatalogueFetched += Initialize;
	}


	public async void Initialize(JSONArray catalogue)
	{
		experiences = Place.Catalogue;

		Tile_Layout_Flex_Horizontal prev = null;

		var i = -1;
		
		foreach (var e in experiences)
		{
			var newTile = Instantiate(original: tilePrefab,
			                          parent: tileParent,
			                          worldPositionStays: false).GetComponent<Tile>();

			newTile.layout.previous = prev;

			prev = newTile.layout;
			
			tiles.Add(newTile);

			await newTile.Populate(e, catalogue[++i]);
		}

		// This is required for Tile scripts to have enough time to initialize themselves.
		
		await Task.Yield();

//		tiles[0].Select();
		
		enabled = true;

//		await Task.Delay(1000);

		ArrangeTileLayout();
	}


//	void Update()
//	{
//		foreach (var t in tiles) t.layout.Arrange();
//	}

	private async void TileSelectedHandler(Tile outgoing,
	                                       Tile incoming)
	{
		if (incoming == null) return;

		while (ReferenceEquals(objA: incoming,
		                       objB: Tile.Selected) &&
		       Mathf.Abs(_x - incoming.layout.relativePosition.x) > 0.1f)
		{
			_x = Mathf.SmoothDamp(current: _x,
			                      target: incoming.layout.relativePosition.x,
			                      currentVelocity: ref _xVel,
			                      smoothTime: xAnimDuration);

			_rect.anchoredPosition = new Vector2(x: -_x,
			                                     y: 0);

			await Task.Yield();
		}
	}
	
	public async void ArrangeTileLayout()
	{
		if (arranging) return;
		
		arranging = true;
		
		Debug.Log("I have started arranging the tiles.");

		do
		{
			foreach (var t in tiles) t.layout.Arrange();

			await Task.Yield();
		} while (tiles.Any(t => t.dirty));
		
//		while (tiles.Any(t => t.dirty))
//		{
//			foreach (var t in tiles) t.layout.Arrange();
//
//			await Task.Yield();
//		}

		Debug.Log("I have stopped arranging the tiles.");

		arranging = false;
	}
	
}