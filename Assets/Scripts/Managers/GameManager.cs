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

	[RPC]
	public void ChangeMode(int newMode) {
		if(Network.isServer ) {
			if( Network.connections.Length > 0 )
				networkView.RPC("ChangeMode", RPCMode.Others, newMode);

			switch( newMode )
			{
			case (int)GameMode.Title:
				m_gameIsRestarting = false;
				break;

			case (int)GameMode.Play:
				//ameraMove.instance.MoveCamAlongSpline();
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

	public void TransitionToLevel( int level ) {
		StaticPool.s_instance.Reset();
		m_cameraMove.Reset();
		m_networkLevelLoader.TransitionToLevel( level );
	}
}