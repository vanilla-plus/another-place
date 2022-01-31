using UnityEngine;

using Vanilla.Easing;

public class Tile_Hover_Group : Tile_Element
{

	public CanvasGroup group;

	public float scaleMin = 1.0f;
	public float scaleMax = 2.0f;


	public override void Awake()
	{
		base.Awake();

		tile.hover.onActiveNormalStart += () => group.gameObject.SetActive(true);

		tile.hover.onActiveNormalFrame   += FadeGroup;
		tile.hover.onInactiveNormalFrame += FadeGroup;

		tile.hover.onActiveNormalFrame   += ScaleGroup;
		tile.hover.onInactiveNormalFrame += ScaleGroup;

		tile.hover.onInactiveNormalComplete += () => group.gameObject.SetActive(false);
	}


	private void FadeGroup(float n) => group.alpha = n.InOutQuadratic();


	private void ScaleGroup(float n) => group.transform.localScale = Vector3.one *
	                                                                 Mathf.Lerp(a: scaleMin,
	                                                                            b: scaleMax,
	                                                                            t: n.InOutQuadratic());

}