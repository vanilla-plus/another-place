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

		tile.onPopulate += (e,
		                    json) => Log($"Tile_Test\t[{tile.experience.title}]\tonPopulate [{e.title}]");

		tile.hover.onActiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\thover.onActiveNormalStart");
		tile.hover.onActiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\thover.onActiveNormalFrame\t[{n}]");
		tile.hover.onActiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\thover.onActiveNormalComplete");

		tile.select.onActiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onActiveNormalStart");
		tile.select.onActiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onActiveNormalFrame\t[{n}]");
		tile.select.onActiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onActiveNormalComplete");

		tile.focus.onActiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onActiveNormalStart");
		tile.focus.onActiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onActiveNormalFrame\t[{n}]");
		tile.focus.onActiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onActiveNormalComplete");

		tile.hover.onInactiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\thover.onInactiveNormalStart");
		tile.hover.onInactiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\thover.onInactiveNormalFrame\t[{n}]");
		tile.hover.onInactiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\thover.onInactiveNormalComplete");

		tile.select.onInactiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onInactiveNormalStart");
		tile.select.onInactiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onInactiveNormalFrame\t[{n}]");
		tile.select.onInactiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tselect.onInactiveNormalComplete");

		tile.focus.onInactiveNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onInactiveNormalStart");
		tile.focus.onInactiveNormalFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onInactiveNormalFrame\t[{n}]");
		tile.focus.onInactiveNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tfocus.onInactiveNormalComplete");
	}

}
