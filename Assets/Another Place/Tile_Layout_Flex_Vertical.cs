using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Tile_Layout_Flex_Vertical : Tile_Layout<Tile_Layout_Flex_Vertical>
{

	public override Vector2 GetStartingPosition() => rect.anchoredPosition;

	public override Vector2 GetOutgoingOffset() => GetTileHalf;

	public override Vector2 GetIncomingOffset() => GetTileHalf;

	public override void ApplyPosition() => rect.anchoredPosition = relativePosition;

	private Vector2 GetTileHalf =>
		new Vector2(x: 0.0f,
		            y: rect.sizeDelta.y * 0.5f + margin);

}