using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILayout<L, P>
	where L : ILayout<L, P>
	where P : struct
{

	L GetPrevious();
	
	P GetStartingPosition();

	P GetRelativePosition();

	P GetOutgoingOffset();

	P GetIncomingOffset();

	void Arrange();

	void UpdateRelativePosition();

	void ApplyPosition();
	
}