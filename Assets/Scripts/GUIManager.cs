using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManager : MonoBehaviour {

    enum GUIManagerState { Title, HUD, GameOverScreen, Dialogue}

	private static GUIManager _instance;

    public Image[] m_startScreens;    
    public Image[] m_endScreens;
    public GameObject dialogue;

	// Fading of GUI elements
	public float m_minAlpha = 0f;
	public float m_maxAlpha = 1f;
	public float m_fadeTime = 1f;

	private HUD hud;
    private GUIManagerState currGUIState;

	#region Singleton Initialization
	public static GUIManager instance {
		get { 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GUIManager>();
			
			return _instance;
		}
	}
	
	void Awake() {
		if(_instance == null) {
			//If I am the fist instance, make me the first Singleton
			_instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			//If a Singleton already exists and you find another reference in scene, destroy it
			if(_instance != this)
				DestroyImmediate(gameObject);
		}
	}
	#endregion

	// Use this for initialization
	void Start () {
		hud = CameraManager.instance.GetComponent<HUD>();

		currGUIState = GUIManagerState.Title;
	}
	
	void OnLevelWasLoaded( int level ) {
		if( level > 1 ) {
			ZeroAlphaOnStartGUIs();
		}
	}

	public void ChangeModeGUI() {
		// Checking what state the game manager is in. Switch statement not working for enum
		if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.Title ) {
			// Activate title screen
			RestartStartScreen();
			networkView.RPC( "RestartStartScreen", RPCMode.Others );
			//if GameOverScreen is up, fade out
			FadeEndScreen( true );
			networkView.RPC( "FadeEndScreen", RPCMode.Others, true );

		} else if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.Play ) {
			// Deactivate title
			FadeStartScreen( true );
			networkView.RPC( "FadeStartScreen", RPCMode.Others, true );

		} else if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.GameOver ) {
			// Activate End Screen
			FadeEndScreen( false );
			networkView.RPC( "FadeEndScreen", RPCMode.Others, false );
		}

//		switch( GameManager.instance.m_currMode )
//		{
//		case GameManager.GameMode.Title:
//			// if GameOverScreen is up, fade out
//			gameOverScreen.enabled = false;
//			// Activate title screen
//			title.enabled = true;
//			break;
//		case GameManager.GameMode.Play:
//			// Deactivate title
//			title.enabled = false;
//			// Activate hearts
//			CameraMove.instance.GetComponent<Stats>().Reset();
//			UpdateHud();
//			break;
//		case GameManager.GameMode.GameOver:
//			// Deactivate hearts
//			CameraMove.instance.GetComponent<Stats>().m_currHealth = 0;
//			UpdateHud();
//			// Activate End Screen
//			gameOverScreen.enabled = true;
//			break;
//		}
	}

	public void UpdateHud() {
		hud.UpdateHp();
	}

	[RPC]
	void ZeroAlphaOnStartGUIs() {
		foreach( Image img in m_startScreens ) {
			if( img != null )
				img.color = new Color( img.color.r, img.color.g, img.color.b, 0f );
		}
		foreach( Image img in m_endScreens ) {
			if( img != null )
				img.color = new Color( img.color.r, img.color.g, img.color.b, 0f );
		}

		if( Network.isServer ) 
			networkView.RPC( "ZeroAlphaOnStartGUIs", RPCMode.Others );
	}

	[RPC]
	void RestartStartScreen() {
		foreach( Image img in m_startScreens ) {
			if( img != null ) {
				img.enabled = true;
				Color color = img.color;
				color.a = 1f;
				img.color = color;
			}
		}
	}

	[RPC]
	public void FadeStartScreen( bool fadeOut ) {
		foreach( Image startScreenImage in m_startScreens ) {
			if( startScreenImage != null ) {
				if( fadeOut ) {
					StartCoroutine( "FadeOut", startScreenImage );
				} else {
					StartCoroutine( "FadeIn", startScreenImage );
				}
			}
		}
	}

	[RPC]
	public void FadeEndScreen( bool fadeOut ) {
		foreach( Image endScreenImage in m_endScreens ) {
			if( endScreenImage != null ) {
				if( fadeOut )
					StartCoroutine( "FadeOut", endScreenImage );
				else
					StartCoroutine( "FadeIn", endScreenImage );
			}
		}
	}

	IEnumerator FadeIn( Image image ) {
		if( image == null ) {
			Debug.Log( gameObject.name + "'s GuiManager is missing an image for fading." );
			yield return null;
		}

		image.enabled = true;

		float timer = 0f;
		Color t_textureColor = image.color;
		float t_startAlpha = image.color.a;
		
		while( timer <= 1f ) {
			t_textureColor = image.color;
			t_textureColor.a = Mathf.Lerp( t_startAlpha, m_maxAlpha, timer);
			image.color = t_textureColor;
			
			timer += Time.deltaTime / m_fadeTime;
			yield return null;
		}

		t_textureColor.a = 1f;
		image.color = t_textureColor;

		if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.GameOver && !GameManager.instance.m_gameIsRestarting && Network.isServer ) {
			GameManager.instance.m_gameIsRestarting = true;
			FadeStartScreen( false );
			networkView.RPC( "FadeStartScreen", RPCMode.Others, false );

			yield return new WaitForSeconds( 8f );
			GameManager.instance.TransitionToLevel( 1 );
		}
	}
	
	IEnumerator FadeOut( Image image ) {
		if( image == null ) {
			Debug.Log( gameObject.name + "'s GuiManager is missing an image for fading." );
			yield return null;
		}

		image.enabled = true;

		float timer = 0f;
		Color t_textureColor;
		float t_startAlpha = image.color.a;
		
		while( timer <= 1f ) {
			t_textureColor = image.color;
			t_textureColor.a = Mathf.Lerp( t_startAlpha, m_minAlpha, timer);
			image.color = t_textureColor;
			
			timer += Time.deltaTime / m_fadeTime;
			yield return null;
		}

		image.enabled = false;
		if( image == m_startScreens[0] && Network.isServer ) {
			yield return new WaitForSeconds( 2f );
			CameraMove.instance.MoveCamAlongSpline();
		}
	}

}
