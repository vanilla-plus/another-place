using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Vanilla.Easing;

public class Tile_Group_Available : Tile_Element
{

    public CanvasGroup group;

    private void Start()
    {
        tile.available.onActiveNormalStart      += () => group.gameObject.SetActive(true);
        tile.available.onInactiveNormalComplete += () => group.gameObject.SetActive(false);

        tile.available.onActiveNormalFrame += n => group.alpha = n.InOutQuadratic();

        tile.available.onInactiveNormalFrame += n => group.alpha = n.InOutQuadratic();
    }

}