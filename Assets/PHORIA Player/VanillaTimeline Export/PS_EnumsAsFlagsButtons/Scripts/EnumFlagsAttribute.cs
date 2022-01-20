using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute
{
	public int numBtnsPerRow;

	public EnumFlagsAttribute() {
		numBtnsPerRow = -1; // Fit as many as possible
	}

	public EnumFlagsAttribute(int numBtnsPerRow) {
		this.numBtnsPerRow = numBtnsPerRow;
	}
}