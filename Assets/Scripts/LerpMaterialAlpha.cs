using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
[RequireComponent (typeof (BoxCollider))]
public class LerpMaterialAlpha : MonoBehaviour {

	Material myMaterial;
	NetworkView myNetworkview;

	public float m_lerpTime = 1f;

	// Use this for initialization
	void Start () {
		myMaterial = renderer.material;
		myNetworkview = GetComponent<NetworkView>();
	}
	
	void OnTriggerEnter( Collider col ) {
		if( Network.isServer ) {
			if( col.tag == "Player" ) {
				StartLerpingAlpha();
				myNetworkview.RPC( "StartLerpingAlpha", RPCMode.Others );
			}
		}
	}

	IEnumerator LerpAlpha() {
		float timer = 0f;
	
		while( timer <= m_lerpTime ) {
			float alpha = Mathf.Lerp( 1f, 0f, timer / m_lerpTime );
			myMaterial.color = new Color( myMaterial.color.r, myMaterial.color.g, myMaterial.color.b, alpha );

			timer += Time.deltaTime;
			yield return null;
		}
		gameObject.SetActive( false );
	}

	[RPC]
	void StartLerpingAlpha() {
		StartCoroutine( LerpAlpha() );
	}
}
