using UnityEngine;
using System.Collections;

public class TrackBall : MonoBehaviour {

	public float sensitivity = 5;

	Vector3 lastMousePosition;

	void Awake() {
		lastMousePosition = Input.mousePosition;
	}

	void Update() {
		if (Input.GetMouseButton(1)) {
			Vector3 mouseDelta = new Vector3(Input.GetAxis("Mouse X"), - Input.GetAxis("Mouse Y"), 0);
			mouseDelta *= sensitivity;
			transform.Rotate(mouseDelta.y, mouseDelta.x, 0, Space.Self);
		}
		lastMousePosition = Input.mousePosition;

	}
}
