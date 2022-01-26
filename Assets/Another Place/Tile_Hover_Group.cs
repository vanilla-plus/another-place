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

		tile.onHoverNormalStart += () => group.gameObject.SetActive(true);

		tile.onHoverNormalFrame   += FadeGroup;
		tile.onDehoverNormalFrame += FadeGroup;

		tile.onHoverNormalFrame   += ScaleGroup;
		tile.onDehoverNormalFrame += ScaleGroup;

		tile.onDehoverNormalComplete += () => group.gameObject.SetActive(false);
	}


	private void FadeGroup(float n) => group.alpha = n.InOutQuadratic();


	private void ScaleGroup(float n) => group.transform.localScale = Vector3.one *
	                                                                 Mathf.Lerp(a: scaleMin,
	                                                                            b: scaleMax,
	                                                                            t: n.InOutQuadratic());

}