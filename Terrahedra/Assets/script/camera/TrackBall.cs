using UnityEngine;
using System.Collections;

public class TrackBall : MonoBehaviour {

	public float sensitivity = 5;

	Vector3 lastMousePosition;

	void Awake() {
		lastMousePosition = Input.mousePosition;
	}

	void Update() {
		if (Input.GetMouseButton(2)) {
			Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
			mouseDelta *= sensitivity;
			transform.Rotate(- mouseDelta.y, mouseDelta.x, 0, Space.Self);
		}
		lastMousePosition = Input.mousePosition;

	}
}
