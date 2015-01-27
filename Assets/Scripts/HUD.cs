using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {
    
    public Image[] canvas0Hearts;
    public Image[] canvas1Hearts;
    public Image[] canvas3Hearts;

	public ParticleSystem canvas0Splat;
	public ParticleSystem canvas1Splat;
	public ParticleSystem canvas3Splat;
	
	public Image saveTheVillagersImage0;
	public Image saveTheVillagersImage1;
	public Image saveTheVillagersImage3;
	public Image fendOffTheHordeImage0;
	public Image fendOffTheHordeImage1;
	public Image fendOffTheHordeImage3;

	private Stats playerStats;

	void Start() {
		playerStats = CameraMove.instance.GetComponent<Stats>();
	}

	void Update() {
		if( Input.GetKeyDown( KeyCode.G ) )
			ActivateFendOffTheHordeGUI();
		if( Input.GetKeyDown( KeyCode.H ) )
			ActivateSaveTheVillagerGUI();
	}

	[RPC]
	public void UpdateHp() {
		// Every time we update hp, go through the list of gui hearts. Turn off the hearts above the current hp the player has
		for( int i = 0; i < playerStats.m_maxHealth; i++ ) {
			if( i > playerStats.m_currHealth - 1 ) {
				canvas0Hearts[i].enabled = false;
				canvas1Hearts[i].enabled = false;
				canvas3Hearts[i].enabled = false;
			} else {
				canvas0Hearts[i].enabled = true;
				canvas1Hearts[i].enabled = true;
				canvas3Hearts[i].enabled = true;
			}
		}
	}

	/// <summary>
	/// Activates the splat on the given Camera
	/// </summary>
	/// <param name="cameraIndex">Camera to activate splat on.</param> 
	[RPC]
	public void ActivateSplat( int cameraIndex ) {
		switch( cameraIndex )
		{
		case 0:
			canvas0Splat.Play();
			break;
		case 1:
			canvas1Splat.Play();
			break;
		case 3:
			canvas3Splat.Play();
			break;
		}
	}
		
	[RPC]
	public void ActivateFendOffTheHordeGUI() {
		if( Network.isServer ) {
			networkView.RPC( "ActivateFendOffTheHordeGUI", RPCMode.Others );
			GameManager.instance.PlaySound( GameManager.instance.m_SfxAudioSource, GameManager.instance.m_fendOffTheHordeSound );
		}

		StartCoroutine( "PulseGUI", fendOffTheHordeImage0 );
		StartCoroutine( "PulseGUI", fendOffTheHordeImage1 );
		StartCoroutine( "PulseGUI", fendOffTheHordeImage3 );
	}

	[RPC]
	public void ActivateSaveTheVillagerGUI() {
		if( Network.isServer ) {
			networkView.RPC( "ActivateSaveTheVillagerGUI", RPCMode.Others );
			GameManager.instance.PlaySound( GameManager.instance.m_SfxAudioSource, GameManager.instance.m_saveTheVillagersSound );
		}

		StartCoroutine( "PulseGUI", saveTheVillagersImage0 );
		StartCoroutine( "PulseGUI", saveTheVillagersImage1 );
		StartCoroutine( "PulseGUI", saveTheVillagersImage3 );
	}

	IEnumerator PulseGUI( Image img ) {
		img.enabled = true;
		Vector3 startSize = img.rectTransform.localScale;
		float timer = 0f;
		float lifeTime = 5f;
		float pulseSpeed = 1f;

		while( true ) {
			if( timer >= lifeTime )
				break;

			// Set the gui to pulse between 1 and 2
			float newScale = Mathf.PingPong( Time.time * pulseSpeed, 0.5f ) + 1f;
			img.rectTransform.localScale = new Vector3( newScale, newScale, newScale );

			timer += Time.deltaTime;
			yield return null;
		}
		
		img.enabled = false;
	}
}