using System.Collections;
using System.Collections.Generic;

using SimpleJSON;

using TMPro;

using UnityEngine;

public class Tile_Text_Markup : Tile_Element
{

	public TextMeshProUGUI element;

	public string key;
	
	public override void Awake()
	{
		base.Awake();

		Populate(e: null,
		         page: tile.experience.node);

//		tile.onPopulate += Populate;
	}


	private void Populate(Experience e,
	                      JSONNode page)
	{
//		tile.onPopulate -= Populate;
		
		var content = page[key].Value;

		if (content == null)
		{
			Debug.LogError($"Could not find a value matching the key [{key}]");
			
			return;
		}
		
		element.text = content;
	}
	

}
