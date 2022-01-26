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

		tile.onHoverNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInStart");

//		tile.onHoverInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInFrame\t[{n}]");
		tile.onHoverNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverInEnd");

		tile.onSelectNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectInStart");

//		tile.onSelectInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectInFrame\t[{n}]");
		tile.onSelectNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectInEnd");

		tile.onFocusNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInStart");

//		tile.onFocusInFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInFrame\t[{n}]");
		tile.onFocusNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusInEnd");

		tile.onDehoverNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutStart");

//		tile.onHoverOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutFrame\t[{n}]");
		tile.onDehoverNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonHoverOutEnd");

		tile.onDeselectNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutStart");

//		tile.onSelectOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutFrame\t[{n}]");
		tile.onDeselectNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonSelectOutEnd");

		tile.onDefocusNormalStart += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutStart");

//		tile.onFocusOutFrame += n => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutFrame\t[{n}]");
		tile.onDefocusNormalComplete += () => Log($"Tile_Test\t[{tile.experience.title}]\tonFocusOutEnd");
	}

}
