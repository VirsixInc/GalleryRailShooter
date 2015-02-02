using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManager : MonoBehaviour {

    enum GUIManagerState { Title, HUD, GameOverScreen, Dialogue}

	private static GUIManager _instance;

	public Text m_titleText;
	public Text m_shootToStartText;
    public Image[] m_startScreens;    
    public Text[] m_endScreens;
	public Image[] m_transitionImages;

	// Fading of GUI elements
	private float m_minAlpha = 0f;
	private float m_maxAlpha = 1f;
	private float m_fadeTime = 1f;

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
		foreach( Text text in m_endScreens ) {
			if( text != null )
				text.color = new Color( text.color.r, text.color.g, text.color.b, 0f );
		}

		if( Network.isServer ) 
			networkView.RPC( "ZeroAlphaOnStartGUIs", RPCMode.Others );
	}

	[RPC]
	void RestartStartScreen() {
		if( m_titleText != null ) {
			m_titleText.enabled = true;
			m_titleText.color = new Color( m_titleText.color.r, m_titleText.color.g, m_titleText.color.b, 1f );
		} else {
			Debug.LogWarning( "Title GUI text is missing!" );
		}

		if( m_shootToStartText != null ) {
			m_shootToStartText.enabled = true;
			m_shootToStartText.color = new Color( m_shootToStartText.color.r, m_shootToStartText.color.g, m_shootToStartText.color.b, 1f );
		} else {
			Debug.Log( "Shoot to Start GUI text is missing!" );
		}
	}

	[RPC]
	public void FadeStartScreen( bool fadeOut ) {
		if( m_titleText != null ) {
			if( fadeOut )
				StartCoroutine( FadeOut( m_titleText ) );
			else
				StartCoroutine( FadeIn( m_titleText ) );
		} else {
			Debug.LogWarning( "Title GUI text is missing!" );
		}

		if( m_shootToStartText != null ) {
			if( fadeOut )
				StartCoroutine( FadeOut( m_shootToStartText ) );
			else
				StartCoroutine( FadeIn( m_shootToStartText ) );
		} else {
			Debug.Log( "Shoot to Start GUI text is missing!" );
		}
	}

	[RPC]
	public void FadeEndScreen( bool fadeOut ) {
		foreach( Text endScreenImage in m_endScreens ) {
			if( endScreenImage != null ) {
				if( fadeOut )
					StartCoroutine( FadeOut( endScreenImage ) );
				else
					StartCoroutine( FadeIn( endScreenImage ) );
			}
		}
	}

	[RPC]
	public void FadeTransitionScreen( bool fadeOut ) {
		foreach( Image transitionImage in m_transitionImages ) {
			if( transitionImage != null ) {
				if( fadeOut )
					StartCoroutine( FadeOut( transitionImage ) );
				else 
					StartCoroutine( FadeIn( transitionImage ) );
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

	IEnumerator FadeIn( Text text ) {
		if( text == null ) {
			Debug.Log( gameObject.name + "'s GuiManager is missing an text for fading." );
			yield return null;
		}
		
		text.enabled = true;
		
		float timer = 0f;
		Color t_textureColor = text.color;
		float t_startAlpha = text.color.a;
		
		while( timer <= 1f ) {
			t_textureColor = text.color;
			t_textureColor.a = Mathf.Lerp( t_startAlpha, m_maxAlpha, timer);
			text.color = t_textureColor;
			
			timer += Time.deltaTime / m_fadeTime;
			yield return null;
		}
		
		t_textureColor.a = 1f;
		text.color = t_textureColor;
		
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
//		if( image == m_startScreens[0] && Network.isServer ) {
//			yield return new WaitForSeconds( 2f );
//			CameraMove.instance.MoveCamAlongSpline();
//		}
	}
	IEnumerator FadeOut( Text text ) {
		if( text == null ) {
			Debug.Log( gameObject.name + "'s GuiManager is missing an image for fading." );
			yield return null;
		}
		
		text.enabled = true;
		
		float timer = 0f;
		Color t_textureColor;
		float t_startAlpha = text.color.a;
		
		while( timer <= 1f ) {
			t_textureColor = text.color;
			t_textureColor.a = Mathf.Lerp( t_startAlpha, m_minAlpha, timer);
			text.color = t_textureColor;
			
			timer += Time.deltaTime / m_fadeTime;
			yield return null;
		}
		
		text.enabled = false;
		if( text == m_titleText && Network.isServer ) {
			yield return new WaitForSeconds( 2f );
			CameraMove.instance.MoveCamAlongSpline();
		}
	}
}
