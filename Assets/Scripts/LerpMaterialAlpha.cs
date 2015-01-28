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
				StartLerpingAlpha( true );
				myNetworkview.RPC( "StartLerpingAlpha", RPCMode.Others, true );
			}
		}
	}

	void OnTriggerExit( Collider col ) {
		if( Network.isServer ) {
			if( col.tag == "Player" ) {
				StartLerpingAlpha( false );
				myNetworkview.RPC( "StartLerpingAlpha", RPCMode.Others, false );
			}
		}
	}

	IEnumerator LerpAlphaOut() {
		float timer = 0f;
	
		while( timer <= m_lerpTime ) {
			float alpha = Mathf.Lerp( 1f, 0f, timer / m_lerpTime );
			myMaterial.color = new Color( myMaterial.color.r, myMaterial.color.g, myMaterial.color.b, alpha );

			timer += Time.deltaTime;
			yield return null;
		}

		renderer.enabled = false;
	}

	IEnumerator LerpAlphaIn() {
		float timer = 0f;
		renderer.enabled = true;
		
		while( timer <= m_lerpTime ) {
			float alpha = Mathf.Lerp( 0f, 1f, timer / m_lerpTime );
			myMaterial.color = new Color( myMaterial.color.r, myMaterial.color.g, myMaterial.color.b, alpha );
			
			timer += Time.deltaTime;
			yield return null;
		}
	}

	[RPC]
	void StartLerpingAlpha( bool lerpToClear ) {
		if( lerpToClear ) {
			StartCoroutine( LerpAlphaOut() );
		} else {
			StartCoroutine( LerpAlphaIn() );
		}
	}
}
