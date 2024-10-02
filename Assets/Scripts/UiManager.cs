using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class UiManager : Singleton<UiManager>
{
	public static readonly Stack<IBack> AllPages = new();

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && AllPages.TryPeek(out var back))
			back.Back();
	}
}
