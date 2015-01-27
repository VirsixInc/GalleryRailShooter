//#define LevelDebug

using UnityEngine;
using System.Collections;
using OSC.NET;


public class ManagerScript : MonoBehaviour {

	private static ManagerScript _instance;

	public enum CameraIndex {Front, Right, Back, Left}

	[System.NonSerialized]
	public NetworkInterpolatedTransform m_NIT;							//Used for NetworkSync. Without this, camera movement will be choppy across network.
	public clientScript m_frontCam, m_rightCam, m_backCam, m_leftCam;
	
	private int m_assignedCameraIndex;
 
	#region Singleton Initialization
	public static ManagerScript instance {
		get { 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<ManagerScript>();
			
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

	void Start () {
		m_frontCam.isAssigned = true;
		m_assignedCameraIndex = -1;
		m_NIT = GetComponent<NetworkInterpolatedTransform>();
	}

	void Update () {
		if( Network.isServer && Input.GetKeyDown(KeyCode.Return ) )
			GameManager.instance.TransitionToLevel( 1 );
	}

	public void InitHost(){
		m_assignedCameraIndex = 0;
	}

	void OnConnectedToServer() {
		//Enable network sync only on clients
		m_NIT.enabled = true;
	}

	void OnGUI() {
		if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.CameraSetup ) {
			if(Network.isClient) {
				//Right Camera button
				if(!m_rightCam.isAssigned) {
					if(GUILayout.Button("Right View")) {
						if(m_assignedCameraIndex != -1)
							networkView.RPC("UnassignCamera", RPCMode.Server, m_assignedCameraIndex);

						networkView.RPC("AssignCamera", RPCMode.Server, (int)CameraIndex.Right);
						m_assignedCameraIndex = (int)CameraIndex.Right;
						ActivateCamera(CameraIndex.Right);
					}
				} else {
					GUILayout.Label("Right View");
				}
				//Back Camera button
				if(!m_backCam.isAssigned) {
					if(GUILayout.Button("Behind View")) {
						if(m_assignedCameraIndex != -1)
							networkView.RPC("UnassignCamera", RPCMode.Server, m_assignedCameraIndex);

						networkView.RPC("AssignCamera", RPCMode.Server, (int)CameraIndex.Back);
						m_assignedCameraIndex = (int)CameraIndex.Back;
						ActivateCamera(CameraIndex.Back);
					}
				} else {
					GUILayout.Label("Behind View");
				}
				//Left Camera button
				if(!m_leftCam.isAssigned) {
					if(GUILayout.Button("Left View")) {
						if(m_assignedCameraIndex != -1)
							networkView.RPC("UnassignCamera", RPCMode.Server, m_assignedCameraIndex);

						networkView.RPC("AssignCamera", RPCMode.Server, (int)CameraIndex.Left);
						m_assignedCameraIndex = (int)CameraIndex.Left;
						ActivateCamera(CameraIndex.Left);
					}
				} else {
					GUILayout.Label("Left View");
					}
				//Unassign camera to free up slot
				if(GUILayout.Button("Unassign Camera")) {
					networkView.RPC("UnassignCamera", RPCMode.Server, m_assignedCameraIndex);
					m_assignedCameraIndex = -1;
				}
			}
#if LevelDebug
			else {
				if(GUILayout.Button("Go!")) {
					GameManager.instance.ChangeMode((int)GameManager.GameMode.Title);//(int)GameManager.GameMode.Play);
					//transform.parent.GetComponent<CameraMove2>().MoveCamAlongSpline();
				}
			}
#endif
		}
	}

	public bool IsCameraAssigned(int index) {
		switch(index)
		{
		case 0:
			return true;
		case 1:
			return m_rightCam.isAssigned;
		case 2:
			return m_backCam.isAssigned;
		case 3:
			return m_leftCam.isAssigned;
		default:
			return false;
		}
	}

	void ActivateCamera(CameraIndex cameraIndex) {
		switch(cameraIndex) 
		{
		case CameraIndex.Right:
			m_frontCam.GetComponent<Camera>().enabled = false;
			m_rightCam.GetComponent<Camera>().enabled = true;
			m_backCam.GetComponent<Camera>().enabled = false;
			m_leftCam.GetComponent<Camera>().enabled = false;
			break;
		case CameraIndex.Back:
			m_frontCam.GetComponent<Camera>().enabled = false;
			m_rightCam.GetComponent<Camera>().enabled = false;
			m_backCam.GetComponent<Camera>().enabled = true;
			m_leftCam.GetComponent<Camera>().enabled = false;
			break;
		case CameraIndex.Left:
			m_frontCam.GetComponent<Camera>().enabled = false;
			m_rightCam.GetComponent<Camera>().enabled = false;
			m_backCam.GetComponent<Camera>().enabled = false;
			m_leftCam.GetComponent<Camera>().enabled = true;
			break;
		}
	}

	public int GetNumAssignedCams() {
		int camsAssigned = 1;

		if(m_rightCam.isAssigned)
			camsAssigned++;
		if(m_backCam.isAssigned)
		   camsAssigned++;
		if(m_backCam.isAssigned)
			camsAssigned++;

		return camsAssigned;
	}

	//The int camera refers to what camera you want to have assigned. 1 = Right, 2 = Behind, 3 = Left
	[RPC]
	void AssignCamera(int cameraIndex) {
		if(Network.isServer) {
			switch(cameraIndex)
			{
			case 1:
				m_rightCam.isAssigned = true;
				networkView.RPC("AssignCamera", RPCMode.Others, 1);
				break;
			case 2:
				m_backCam.isAssigned = true;
				networkView.RPC("AssignCamera", RPCMode.Others, 2);
				break;
			case 3:
				m_leftCam.isAssigned = true;
				networkView.RPC("AssignCamera", RPCMode.Others, 3);
				break;
			}
		} 
		else if (Network.isClient) {			
			switch(cameraIndex)
			{
			case 1:
				m_rightCam.isAssigned = true;
				break;
			case 2:
				m_backCam.isAssigned = true;
				break;
			case 3:
				m_leftCam.isAssigned = true;
				break;
			}
		}
	}

	[RPC]
	void UnassignCamera(int cameraIndex) {
		if(Network.isServer) {
			switch(cameraIndex)
			{
			case 1:
				m_rightCam.isAssigned = false;
				networkView.RPC("UnassignCamera", RPCMode.Others, 1);
				break;
			case 2:
				m_backCam.isAssigned = false;
				networkView.RPC("UnassignCamera", RPCMode.Others, 2);
				break;
			case 3:
				m_leftCam.isAssigned = false;
				networkView.RPC("UnassignCamera", RPCMode.Others, 3);
				break;
			}
		} 
		else if (Network.isClient) {			
			switch(cameraIndex)
			{
			case 1:
				m_rightCam.isAssigned = false;
				break;
			case 2:
				m_backCam.isAssigned = false;
				break;
			case 3:
				m_leftCam.isAssigned = false;
				break;
			}
		}
	}

	public void OSCMessageReceived(OSC.NET.OSCMessage msg){
		ArrayList args = msg.Values;
		ShotManager.instance.NetworkShoot((float)args[0], (1f-(float)args[1]));
		print (args[0] + "    " + args[1]);
	}
}
	