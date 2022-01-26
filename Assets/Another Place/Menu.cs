using System.Collections.Generic;
using System.Threading.Tasks;

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

	public List<Tile> tiles = new List<Tile>();

	private void Awake()
	{
		i = this;
		
		_x = 0.0f;

		Tile.OnSelectedChanged += TileSelectedHandler;
		
		_rect = (RectTransform)tileParent.transform;

		_rect.anchoredPosition = Vector2.zero;

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

//		Time.timeScale = 0.1f;

		await Task.Yield(); // Give it a frame for scripts to init

		tiles[0].Select();
		
		enabled = true;
		
	}


	void Update()
	{
		foreach (var t in tiles) t.Arrange();
	}

	private async void TileSelectedHandler(Tile outgoing,
	                                       Tile incoming)
	{
		if (incoming == null) return;

		while (ReferenceEquals(objA: incoming,
		                       objB: Tile.Selected) &&
		       Mathf.Abs(_x - incoming.xPosition) > 0.1f)
		{
			_x = Mathf.SmoothDamp(current: _x,
			                      target: incoming.xPosition,
			                      currentVelocity: ref _xVel,
			                      smoothTime: xAnimDuration);

			_rect.anchoredPosition = new Vector2(x: -_x,
			                                     y: 0);

			await Task.Yield();
		}
	}

}