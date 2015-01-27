using UnityEngine;
using System.Collections;

public class ZoRaSh_Debug : MonoBehaviour {

	bool m_showDebugOptions = false;

	private NavMeshAgent m_playerNavmeshAgent;

	void Start() {
		m_playerNavmeshAgent = CameraMove.instance.GetComponent<NavMeshAgent>();
	}

	void Update() {
		if( Input.GetKeyDown( KeyCode.BackQuote ) )
		   m_showDebugOptions = !m_showDebugOptions;

		if( Network.isServer ) {
			if( Input.GetKeyDown( KeyCode.F1 ) )
				GameManager.instance.GOD_MODE = !GameManager.instance.GOD_MODE;
		}
		
		if( Input.GetKeyDown( KeyCode.F2 ) )
			ShotManager.instance.INVERT_X = !ShotManager.instance.INVERT_X; 
		if( Input.GetKeyDown( KeyCode.F3 ) )
			ShotManager.instance.INVERT_Y = !ShotManager.instance.INVERT_Y; 
	}

	void OnGUI() {
		if( m_showDebugOptions ) {
			GUILayout.BeginVertical();
			GUILayout.Label( "F1: God Mode = " + GameManager.instance.GOD_MODE.ToString() );
			GUILayout.Label( "F2: Invert input X = " + ShotManager.instance.INVERT_X.ToString() );
			GUILayout.Label( "F3: Invert input Y = " + ShotManager.instance.INVERT_Y.ToString() );

			if( Network.isServer ) {
				GUILayout.BeginHorizontal();
				GUILayout.Label( "Player Speed: " );
				m_playerNavmeshAgent.speed = float.Parse( GUILayout.TextField( m_playerNavmeshAgent.speed.ToString() ) );
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}
	}
}
