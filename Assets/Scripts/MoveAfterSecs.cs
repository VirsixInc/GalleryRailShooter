using UnityEngine;
using System.Collections;

public class MoveAfterSecs : MonoBehaviour {

	public float m_waitSeconds;

	// Use this for initialization
	void Start () {
		StartCoroutine( Move() );
	}
	
	IEnumerator Move() {
		yield return new WaitForSeconds( 0.5f );
		CameraMove.instance.MoveCamAlongSpline();
		yield return new WaitForSeconds( m_waitSeconds );
		GUIManager.instance.FadeTransitionScreen( true );
	}
}
