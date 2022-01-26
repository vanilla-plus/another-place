using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public abstract class Tile_Element : MonoBehaviour
{

	protected Tile tile;

	public virtual void Awake() => tile = GetComponentInParent<Tile>();

}
