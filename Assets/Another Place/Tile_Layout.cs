using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public abstract class Tile_Layout<L> : Tile_Element,
                                       ILayout<L, Vector2>
    where L : Tile_Layout<L>
{

    public RectTransform rect;

    public L previous;

    public L GetPrevious() => previous;

    public Vector2 relativePosition;

    public Vector2 GetRelativePosition() => relativePosition;

    public float margin = 25.0f;


    public override void Awake()
    {
        base.Awake();

        rect = (RectTransform)transform;
    }


    public void Arrange()
    {
        UpdateRelativePosition();

        ApplyPosition();
    }


    // E.g. for a horizontal layout:
    // My relative position is the relative position of the previous tile plus half of its width and half of my width (and some margin)
    public void UpdateRelativePosition() => relativePosition = previous ? previous.relativePosition + previous.GetOutgoingOffset() + GetIncomingOffset() : GetStartingPosition();

    public abstract void ApplyPosition();


    public abstract Vector2 GetStartingPosition();



    /// <summary>
    ///     The pixel offset to be used 
    /// </summary>
    /// <returns></returns>
    public abstract Vector2 GetOutgoingOffset();


    public abstract Vector2 GetIncomingOffset();

}