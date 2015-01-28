using UnityEngine;
using System.Collections;

public class LerpMaterialAlpha : MonoBehaviour {

	Material myMaterial;

	public float m_lerpTime = 1f;

	// Use this for initialization
	void Start () {
		myMaterial = renderer.material;
	}
	
	void OnTriggerEnter( Collider col ) {
		if( col.tag == "Player" )
			StartCoroutine( LerpAlpha() );
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
}
