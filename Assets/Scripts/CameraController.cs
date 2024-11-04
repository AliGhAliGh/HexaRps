using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 10f;
	[SerializeField] private float fastMoveSpeed = 30f;
	[SerializeField] private float rotationSpeed = 3f;
	[SerializeField] private float yaw;
	[SerializeField] private float pitch;

	private void Start()
	{
		yaw = transform.eulerAngles.y;
		pitch = transform.eulerAngles.x;
	}

	private void Update()
	{
		var speed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

		if (Input.GetKey(KeyCode.W))
			transform.position += transform.forward * (speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.S))
			transform.position -= transform.forward * (speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.A))
			transform.position -= transform.right * (speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.D))
			transform.position += transform.right * (speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.E))
			transform.position += transform.up * (speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.Q))
			transform.position -= transform.up * (speed * Time.deltaTime);

		if (Input.GetMouseButton(0))
		{
			var mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
			var mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
			yaw += mouseX;
			pitch -= mouseY;
			pitch = Mathf.Clamp(pitch, -89f, 89f);
			transform.eulerAngles = new Vector3(pitch, yaw, 0f);
		}
	}
}
