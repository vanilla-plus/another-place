using System.Collections;
using System.Collections.Generic;

using static UnityEngine.Debug;

public class Tile_Test : Tile_Element
{

	private static bool _someonesGotIt = false;
	
	void Start()
	{
		if (!_someonesGotIt)
		{
			_someonesGotIt = true;

			Tile.OnSelectedChanged += (outgoing,
			                           incoming) => Log(outgoing == null ?
				                                            $"Tile\tOnSelectedChanged\t[null] => [{incoming.experience.title}]" :
				                                            $"Tile\tOnSelectedChanged\t[{outgoing.experience.title}] => [{incoming.experience.title}]");
		}
		
		tile.onPopulate   += e => Log($"Tile_Test\t[{tile.experience.title}]\tonPopulate [{e.title}]");
		
		tile.onHoverInStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInStart");
		tile.onHoverInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInFrame\t[{n}]");
		tile.onHoverInEnd   += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInEnd");
		
		tile.onSelectInStart += () => LogError($"Tile_Test\t[{tile.experience.title}]\tonSelectInStart");
		tile.onSelectInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectInFrame\t[{n}]");
		tile.onSelectInEnd   += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectInEnd");
		
		tile.onFocusInStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInStart");
		tile.onFocusInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInFrame\t[{n}]");
		tile.onFocusInEnd += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInEnd");
		
		tile.onHoverOutStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutStart");
		tile.onHoverOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutFrame\t[{n}]");
		tile.onHoverOutEnd   += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutEnd");
		
		tile.onSelectOutStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutStart");
		tile.onSelectOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutFrame\t[{n}]");
		tile.onSelectOutEnd   += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutEnd");
		
		tile.onFocusOutStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutStart");
		tile.onFocusOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutFrame\t[{n}]");
		tile.onFocusOutEnd   += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutEnd");
	}
}
