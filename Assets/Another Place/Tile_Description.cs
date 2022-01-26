using System.Collections;
using System.Collections.Generic;

using TMPro;

public class Tile_Description : Tile_Element
{

//	public
	
	public override void Awake()
	{
		base.Awake();

		var text = GetComponent<TextMeshProUGUI>();

		tile.onPopulate += (e,
		                    json) =>
		                   {
			                   text.text = e.description;
		                   };
	}

//
//	void Populate(Experience e)
//	{
//		
//	}

}