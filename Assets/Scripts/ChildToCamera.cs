using UnityEngine;
using System.Collections;

public class ChildToCamera : MonoBehaviour {

	void Start() {
		Transform newParent = CameraManager.instance.transform;
		transform.parent = newParent;
		transform.localPosition = Vector3.up * 15f;
	}
}
