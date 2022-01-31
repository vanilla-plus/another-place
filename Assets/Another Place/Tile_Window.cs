#undef DEBUG

using System.Threading.Tasks;

using UnityEngine;

using Vanilla.Easing;

using static UnityEngine.Debug;

public class Tile_Window : Tile_Element
{

	public RectTransform rect;

	public override void Awake()
	{
		base.Awake();

		tile.select.onActiveNormalStart += () =>
		{
			tile.dirty = true;

			Menu.i.ArrangeTileLayout();
		};

		tile.select.onInactiveNormalStart += () =>
		{
			tile.dirty = true;
			
			Menu.i.ArrangeTileLayout();
		};

		tile.select.onActiveNormalComplete += () => tile.dirty = false;
		
		tile.select.onInactiveNormalComplete += () => tile.dirty = false;
		
		tile.select.onActiveNormalFrame += SizeFrameHandler;
		tile.select.onInactiveNormalFrame += SizeFrameHandler;

	}

	public void SizeFrameHandler(float n) => rect.sizeDelta =
		new Vector2(Mathf.Lerp(tile.minWindowSize, tile.maxWindowSize, InOutQuadratic(n)), rect.sizeDelta.y);
	
	public float InOutQuadratic
	(
		float t)
	{
		var m = t - 1;

		var p = t * 2;

		if (p < 1) return t * p;

		return 1 - m * m * 2;
	}
}