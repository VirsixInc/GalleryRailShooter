using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	private static NetworkManager _instance;

	public string CONNECT_IP = "127.0.0.1";
	public int LISTENPORT = 2500;


	#region Singleton Initialization
	public static NetworkManager instance {
		get { 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<NetworkManager>();
			
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

	void OnServerInitialized(){
		Debug.Log("Server Initialized");
		ManagerScript.instance.InitHost();
	}

	void OnConnectedToServer() {
		ManagerScript.instance.m_NIT.enabled = true;
		CameraMove.instance.m_NIT.enabled = true;
	}

	void OnDisconnectedFromServer() {
		if( Network.isClient ) {
			ManagerScript.instance.m_NIT.enabled = false;
			CameraMove.instance.m_NIT.enabled = false;
		}
	}

	void OnGUI() {
		if(GameManager.instance.CurrentMode == (int)GameManager.GameMode.NetworkSetup) {
			if( Network.peerType == NetworkPeerType.Disconnected ) {
				CONNECT_IP = GUILayout.TextField( CONNECT_IP );
				LISTENPORT = int.Parse( GUILayout.TextField( LISTENPORT.ToString() ) );

				if(GUILayout.Button("Start Server")){
					Network.InitializeServer(3, LISTENPORT, Network.HavePublicAddress());
				}
				if(GUILayout.Button("Connect to Server")){
					Network.Connect(CONNECT_IP, LISTENPORT);
				}
			} else {
				if( Network.isServer ) {
					if(GUILayout.Button("Connections Completed"))
						CloseConnections();
				}
			}
		}
	}

	[RPC]
	void CloseConnections() {
		GameManager.instance.ChangeMode((int)GameManager.GameMode.CameraSetup);
	}
}
