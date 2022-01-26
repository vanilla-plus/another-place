using UnityEngine;

using Vanilla.Easing;

public class Tile_Select_Group : Tile_Element
{

	public CanvasGroup group;

	public float scaleMin = 1.0f;
	public float scaleMax = 1.0f;


	public override void Awake()
	{
		base.Awake();

		tile.onSelectNormalStart += () => group.gameObject.SetActive(true);

		tile.onSelectNormalFrame   += FadeGroup;
		tile.onDeselectNormalFrame += FadeGroup;

		tile.onSelectNormalFrame   += ScaleGroup;
		tile.onDeselectNormalFrame += ScaleGroup;

		tile.onDeselectNormalComplete += () => group.gameObject.SetActive(false);
	}


	private void FadeGroup(float n) => group.alpha = n.InOutQuadratic();


	private void ScaleGroup(float n) => group.transform.localScale = Vector3.one *
	                                                                 Mathf.Lerp(a: scaleMin,
	                                                                            b: scaleMax,
	                                                                            t: n.InOutQuadratic());

}