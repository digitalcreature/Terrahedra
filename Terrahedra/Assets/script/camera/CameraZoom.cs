using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour {

	public float zoomMin = 2;
	public float zoomMax = 15;
	public float sensitivity = 1;
	public float smoothing = 15;

	float zoomTarget;

	float zoom {
		get {
			return - transform.localPosition.z;
		}
		set {
			Vector3 pos = transform.localPosition;
			pos.z = - value;
			transform.localPosition = pos;
		}
	}

	void Awake() {
		zoomTarget = zoom;
	}

	void Update() {
		zoomTarget -= Input.mouseScrollDelta.y * sensitivity;
		zoomTarget = Mathf.Clamp(zoomTarget, zoomMin, zoomMax);
		zoom = Mathf.Lerp(zoom, zoomTarget, Time.deltaTime * smoothing);
	}


}
