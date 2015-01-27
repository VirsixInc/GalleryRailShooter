using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	private static GameManager _instance;

	public enum GameMode {NetworkSetup, CameraSetup, Title, Play, GameOver}
	public enum Difficulty {Easy, Medium, Hard}
	
	public Difficulty m_difficulty = Difficulty.Easy;
	public int CurrentDifficulty {get {return (int)m_difficulty;}}

	public GameMode m_currMode = GameMode.NetworkSetup;
	public int CurrentMode {get {return (int)m_currMode;}}

	[System.NonSerialized]
	public bool m_gameIsRestarting = false;

	private CameraManager m_managerScript;
	private CameraMove m_cameraMove;
	private GUIManager m_guiManager;
	private NetworkLevelLoader m_networkLevelLoader;

	// Music
	private bool m_playingGameMusic = false;
	private float m_startMusicVolume;
	public float m_musicFadeTime = 3f;
	private int m_currentSong = 0;
	public AudioSource m_gameMusicAudioSource;
	public AudioClip m_menuSong;
	public AudioClip m_songSongForLimeric;
	public AudioClip[] m_gameSongs;
	// Voice Over
	public AudioSource m_voiceOverAudioSource;
	public AudioClip m_startLimeric;
	public AudioClip m_saveTheVillagersSound;
	public AudioClip m_fendOffTheHordeSound;
	// SFX
	public AudioSource m_SfxAudioSource;
	public AudioClip m_thunderSound;
	public AudioClip m_laughSound;

	// Debug
	public bool GOD_MODE = false;

	#region Singleton Initialization
	public static GameManager instance {
		get { 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GameManager>();
			
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
				//Destroy(gameObject);
		}
	}
	#endregion

	// Use this for initialization
	void Start () {
		m_managerScript = CameraManager.instance;
		m_cameraMove = CameraMove.instance;
		m_guiManager = GUIManager.instance;
		m_networkLevelLoader = GetComponent<NetworkLevelLoader>();
		m_startMusicVolume = m_gameMusicAudioSource.volume;
	}
	
	// Update is called once per frame
	void Update () {
		if( Network.isServer ) {
			// Song logic
//			if( m_playingGameMusic ) {
//				if( m_gameMusicAudioSource.time >= m_gameMusicAudioSource.clip.length - m_musicFadeTime )
//					StartCoroutine( "TransitionToNextSong" );
//			}
		}
	}

	void OnLevelWasLoaded( int level ) {
		if( Network.isServer ) {
			switch( m_currMode ) {
			case GameMode.Title:
				m_gameMusicAudioSource.clip = m_menuSong;
				m_gameMusicAudioSource.Play();
				break;
			}
		}
	}

	[RPC]
	public void ChangeMode(int newMode) {
		if(Network.isServer ) {
			if( Network.connections.Length > 0 )
				networkView.RPC("ChangeMode", RPCMode.Others, newMode);

			switch( newMode )
			{
			case (int)GameMode.Title:
				m_gameIsRestarting = false;
				//m_SfxAudioSource.clip = m_thunderSound;

				if( !m_gameMusicAudioSource.isPlaying )
					m_gameMusicAudioSource.Play();
				break;

			case (int)GameMode.Play:
				CameraMove.instance.MoveCamAlongSpline();
				//StartCoroutine( "TransitionTitleSongToLimericSong" );
				break;

			case (int)GameMode.GameOver:
				GameOver();
				break;
			}
//			if( newMode == (int)GameMode.Title && !m_gameMusicAudioSource.isPlaying )
//				m_gameMusicAudioSource.Play();
//
//			// Sound
//			if( newMode == (int)GameMode.Play ) {
//				StartCoroutine( "TransitionTitleSongToLimericSong" );
//			}
		}

		m_currMode = (GameMode)newMode;
		m_guiManager.ChangeModeGUI();
	}

	IEnumerator TransitionToNextSong() {
		float timer = 0f;

		// Fade out first clip
		while( timer <= 1f ) {
			m_gameMusicAudioSource.volume = Mathf.Lerp( m_startMusicVolume, 0f, timer );
			
			timer += Time.deltaTime / m_musicFadeTime;
			yield return null;
		}

		yield return new WaitForSeconds( 0.5f );
		timer = 0f;

		// Fade in second clip
		m_currentSong++;
		if( m_currentSong >= m_gameSongs.Length )
			m_currentSong = 0;

		m_gameMusicAudioSource.clip = m_gameSongs[m_currentSong];
		m_gameMusicAudioSource.Play();

		while( timer <= 1f ) {
			m_gameMusicAudioSource.volume = Mathf.Lerp( 0f, m_startMusicVolume, timer );
			
			timer += Time.deltaTime / m_musicFadeTime;
			yield return null;
		}
	}

	IEnumerator TransitionTitleSongToLimericSong() {
		float timer = 0f;

		m_gameMusicAudioSource.Stop();
		//m_SfxAudioSource.Play();

		// Fade in second clip
		m_gameMusicAudioSource.clip = m_songSongForLimeric;
		m_gameMusicAudioSource.Play();

		while( timer <= 1f ) {
			m_gameMusicAudioSource.volume = Mathf.Lerp( 0f, m_startMusicVolume, timer );			
			timer += Time.deltaTime / m_musicFadeTime;
			yield return null;
		}

		// Start Limeric
		yield return new WaitForSeconds( 6.3f - m_musicFadeTime );
		m_voiceOverAudioSource.Play();

		yield return new WaitForSeconds( m_voiceOverAudioSource.clip.length + 1f );

		yield return StartCoroutine( "TransitionToGameMusic" );
	}

	IEnumerator TransitionToGameMusic() {
		float timer = 0f;
		
		// Fade out first clip
		while( timer <= 1f ) {
			m_gameMusicAudioSource.volume = Mathf.Lerp( m_startMusicVolume, 0f, timer );
			
			timer += Time.deltaTime / m_musicFadeTime;
			yield return null;
		}

		timer = 0f;

		// Fade in second clip
		m_gameMusicAudioSource.clip = m_gameSongs[0];
		m_gameMusicAudioSource.volume = 1f;
		m_gameMusicAudioSource.Play();
		
//		while( timer <= 1f ) {
//			m_gameMusicAudioSource.volume = Mathf.Lerp( 0f, m_startMusicVolume, timer );
//			
//			timer += Time.deltaTime / m_musicFadeTime;
//			yield return null;
//		}

		m_playingGameMusic = true;
		m_currentSong++;
	}

	/// <summary>
	/// Plaies the sound.
	/// </summary>
	/// <param name="source">Audio source to be used. "SFX" for sound effects, "VO" for voice overs, and "Music" for music.</param>
	/// <param name="newClip">New clip.</param>
	public void PlaySound( AudioSource src, AudioClip newClip ) {
		src.clip = newClip;
		src.Play();
	}

	[RPC]
	void GameOver() {
		if( Network.isServer )
			networkView.RPC( "GameOver", RPCMode.Others );

		StaticPool.s_instance.Reset();
		StaticPool.s_instance.networkView.RPC( "Reset", RPCMode.OthersBuffered );
	}

	void Reset() {
		if( Network.isServer ) {
			StopCoroutine("TransitionToGameMusic");
			StopCoroutine("TransitionTitleSongToLimericSong");
			StopCoroutine("TransitionToNextSong");

			AudioSource[] audios = FindObjectsOfType<AudioSource>();
			foreach( AudioSource aS in audios )
				aS.Stop();

			m_gameMusicAudioSource.clip = m_menuSong;
			m_playingGameMusic = false;
			m_currentSong = 0;
		}
	}

	public void TransitionToLevel( int level ) {
		Reset();
		StaticPool.s_instance.Reset();
		m_cameraMove.Reset();
		m_networkLevelLoader.TransitionToLevel( level );
	}
}