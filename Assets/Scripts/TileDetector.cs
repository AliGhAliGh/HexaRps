using UnityEngine;

public class TileDetector : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;

	// private void Update()
	// {
	// 	if (Input.GetMouseButtonDown(0))
	// 	{
	// 		_ray = mainCamera.ScreenPointToRay(Input.mousePosition);
	//
	// 		if (Physics.Raycast(_ray, out _hit))
	// 		{
	// 			var hitPoint = _hit.point;
	// 			GroundManager.SetColor(ColorMode.Red,GroundManager.GetPosition(hitPoint));
	// 		}
	// 	}
	// }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hit))
				GameManager.GroundClick(GroundManager.GetPosition(hit.point));
		}
		//
		// if (Input.GetMouseButtonDown(1))
		// {
		// 	var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		// 	if (Physics.Raycast(ray, out var hit))
		// 		GroundManager.RemoveBlock(GroundManager.GetPosition(hit.point));
		// }
		//
		// if (Input.GetMouseButtonDown(2))
		// {
		// 	var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		// 	if (Physics.Raycast(ray, out var hit))
		// 		GroundManager.Pushback(GroundManager.GetPosition(hit.point));
		// }
	}
}
