using UnityEngine;
using System.Collections;

public class LerpLook : MonoBehaviour {

	Transform m_lookDir;
	Transform m_player;

	public float m_lerpTime = 3f;

	// Use this for initialization
	void Start () {
		m_lookDir = transform.GetChild( 0 );
		m_player = CameraManager.instance.transform;
	}
	
	void OnTriggerEnter( Collider col ) {
		if( col.tag == "Player" ) {
			StartCoroutine( Lerp() ); 
		}
	}

	IEnumerator Lerp() {
		float timer = 0f;
		while( timer <= m_lerpTime ) {
			Quaternion endRot = Quaternion.FromToRotation( m_player.forward, m_lookDir.position - m_player.position );
			m_player.rotation = Quaternion.Lerp( m_player.rotation, endRot, timer/m_lerpTime );
			timer += Time.deltaTime;
			yield return null;
		}
	}
}
