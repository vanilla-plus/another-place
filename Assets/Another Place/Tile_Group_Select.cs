using UnityEngine;

using Vanilla.Easing;

public class Tile_Group_Select : Tile_Element
{

	public CanvasGroup group;

	public float scaleMin = 1.0f;
	public float scaleMax = 1.0f;


	public override void Awake()
	{
		base.Awake();

		tile.select.onActiveNormalStart += () => group.gameObject.SetActive(true);

		tile.select.onActiveNormalFrame += FadeGroup;
		tile.@select.onInactiveNormalFrame += FadeGroup;

		tile.@select.onActiveNormalFrame   += ScaleGroup;
		tile.@select.onInactiveNormalFrame += ScaleGroup;

		tile.@select.onInactiveNormalComplete += () => group.gameObject.SetActive(false);
	}


	private void FadeGroup(float n) => group.alpha = n.InOutQuadratic();


	private void ScaleGroup(float n) => group.transform.localScale = Vector3.one *
	                                                                 Mathf.Lerp(a: scaleMin,
	                                                                            b: scaleMax,
	                                                                            t: n.InOutQuadratic());

}