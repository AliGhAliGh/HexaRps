using UnityEngine;
using Utilities;

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
			{
				var res = GroundManager.GetPosition(hit.point);
				print(res);
				GroundManager.SetColor(ColorMode.Red, res);
				Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2.0f);
			}
			else
			{
				Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 2.0f);
			}
		}
	}
}
