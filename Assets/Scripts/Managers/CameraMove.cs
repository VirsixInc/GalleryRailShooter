//#define LevelDebug

using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
		
	private static CameraMove _instance;

	private Transform m_thisTransform;
	//[System.NonSerialized]
	public bool m_isMoving;
	
	[System.NonSerialized]
	public NavMeshAgent m_navMeshAgent;
	[System.NonSerialized]
	public NetworkInterpolatedTransform m_NIT;

	public Waypoint m_levelStartWp;
	public Waypoint m_levelEndWp;
	public Waypoint m_nextWaypoint;
	
	public float m_timeSinceEvent = 0f;

	///Variables from debug camera
	public Waypoint waypoint;
	private bool waiting = false;
	public WaveManager currentWave; //set to the next waypoints wavemanager if exists on arrival. remains the same until ariving at another waypoint with a wave
	///end variables from debug camera

	public int camsActive = 0;
	public int camerasReady = 0;

	private Stats m_stats;
	public bool isMovingEvent = false;
	public ManagerScript cameraManager;

	#region Singleton Initialization
	public static CameraMove instance {
		get { 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<CameraMove>();
			
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
		m_thisTransform = GetComponent<Transform>();
		m_navMeshAgent = GetComponent<NavMeshAgent>();
		m_NIT = GetComponent<NetworkInterpolatedTransform>();
		m_stats = GetComponent<Stats>();
		cameraManager = GetComponentInChildren<ManagerScript>();
#if !LevelDebug
	}
		
	void OnLevelWasLoaded( int level ) {
#endif
		m_navMeshAgent.enabled = false;

		if( Network.isServer ) {
			MoveToLevelStart();
			ManagerScript.instance.collider.enabled = true;

			if( level > 0 ) {
				m_navMeshAgent.enabled = true;
				GameManager.instance.ChangeMode( (int)GameManager.GameMode.Title );

				// Check if we have set the beginning or end of the the level. If not, cry.
				if( m_levelStartWp == null ) {
					m_levelStartWp = GameObject.FindGameObjectWithTag( "LevelStart" ).GetComponent<Waypoint>();
					waypoint = m_levelStartWp;


					if( m_levelStartWp == null )
							Debug.LogError( "CameraMove manager: " + name + " is missing its LevelStart waypoint." );
				}
				if( m_levelEndWp == null ) {
					m_levelEndWp = GameObject.FindGameObjectWithTag( "LevelEnd" ).GetComponent<Waypoint>();
					
					if( m_levelEndWp == null )
						Debug.LogError( "CameraMove manager: " + name + " is missing its LevelEnd waypoint." );
				}
				
				PathCompleteCheck();

				m_nextWaypoint = m_levelStartWp.m_next;
				waypoint = m_nextWaypoint;
				CameraCounter();
			}
#if !LevelDebug		
		} else {
			if( Network.peerType == NetworkPeerType.Disconnected )
				MoveToLevelStart();
		} 



#endif
	}
	
	// Update is called once per frame
	void Update () {

		//Update from camerammove
//		if(!something && Application.loadedLevel == 1){
//			MoveToLevelStart();
//		}
//		if( m_isMoving ) {
//			if( Vector3.Distance( m_thisTransform.position, m_nextWaypoint.transform.position ) < 2f 
//			   && m_nextWaypoint.isTimedEvent && timeSinceEvent < m_nextWaypoint.eventTime){
//				StopMovement();
//				timeSinceEvent += Time.deltaTime;
//			} else {
//				if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.Play ) {
//					if( Vector3.Distance( m_thisTransform.position, m_nextWaypoint.transform.position ) < 2f ) {
//						if( m_nextWaypoint == m_levelEndWp ) {
//							StopMovement();
//						} else {
//							m_nextWaypoint = m_nextWaypoint.m_next;
//							MoveCamAlongSpline();
//						}
//					}
//				}
//			}
//		}

		if(m_isMoving && waypoint != null){
			Move();										// TODO Matt think if a better way to make character move. As of right now, Move() is being called every frame
			//MoveCamAlongSpline(); //rpc move. 
			//WaveCheck();
			NextWaypointCheck();
		}
		else if(waiting){
			//WaveCompletionCheck();
			WaveCompletionCounterCheck();
		}
	}

	void StopMovement() {
		m_navMeshAgent.Stop();
	}
	
	void PathCompleteCheck() {
		Waypoint temp = m_levelStartWp;
		
		while( temp != null ) {
			// If we run into a point that doesn't have a next and it is LevelEnd, we're good. Otherwise, cry harder.
			if( temp.m_next == null ) {
				if( temp == m_levelEndWp ) {
					Debug.Log( "Path complete." );
					return;
				} else {
					Debug.LogError( "Path is not complete. " + temp.name + " doesn't have a next WayPoint" );
					return;
				}
			}
			
			// Make sure we aren't in an infinite loop
			if( temp.m_next.m_checked ) {
				Debug.LogError( "Infinite loop detected. " + temp.name + " loops back to " + temp.m_next );
				return;
			}
			
			// Activate the flag that says that this object has been checked for path completion
			temp.m_checked = true;
			
			temp = temp.m_next;
		}
	}

	public void MoveToLevelStart() {
		Debug.Log("Move To Level Start");
		Transform levelStart = GameObject.FindWithTag( "LevelStart" ).transform;
		Transform lookDir =  levelStart.GetChild(0).transform;
		m_thisTransform.position = levelStart.position;
		m_thisTransform.LookAt( new Vector3( lookDir.position.x, m_thisTransform.position.y, lookDir.position.z ) );
		ManagerScript.instance.transform.localRotation = Quaternion.identity;
		m_navMeshAgent.enabled = true;
	}
	
	public static void StartMovement() {
		if(Network.isServer)
			_instance.m_navMeshAgent.SetDestination( _instance.m_nextWaypoint.transform.position );
	}

	public void MoveCamAlongSpline () {
		if( Network.isServer ) {
			m_navMeshAgent.SetDestination( m_nextWaypoint.transform.position );
			m_isMoving = true;
		}
	}

	public void Reset() {
		m_isMoving = false;
		m_levelStartWp = null;
		m_levelEndWp = null;
		m_nextWaypoint = null;
		camsActive = 0;
		camerasReady = 0;
	}

	///Functions from debug camera
	void Move(){
		m_navMeshAgent.SetDestination(waypoint.transform.position);
	}

	void NextWaypointCheck(){
//		if(Vector3.Distance(m_thisTransform.position, new Vector3(waypoint.transform.position.x, m_thisTransform.position.y, waypoint.transform.position.z)) <= 2f){
//			if(WaveCheck())
//				return;
//
//			waypoint = waypoint.m_next;			
//		}
	}

//	bool WaveCheck(){
//		if(waypoint.gameObject.GetComponent<WaveManager>() != null){
//			currentWave = waypoint.GetComponent<WaveManager>();
////			if(!currentWave.isMovingEvent){
////				//currentWave = waypoint.GetComponent<WaveManager>();
////				m_navMeshAgent.Stop();
////				m_isMoving = false;
////				waiting = true;
////				waypoint = waypoint.m_next;
////				return true;
////			}
////			else{
////				//currentWave = waypoint.GetComponent<WaveManager>();
////				waypoint = waypoint.m_next;
////				return true;
////			}
//		}
//		else{
//			return false;
//		}
//	}

	void WaveCompletionCheck(){
//		if(currentWave.villagersComplete && currentWave.waveComplete){
//			m_isMoving = true;
//			currentWave.deactivatePortals();
//		}
//		
//		if(currentWave.waveComplete && currentWave.villagersInWave.Length == 0){
//			m_isMoving = true;
//		}
	}

	void WaveCompletionCounterCheck(){
		if(camsActive == camerasReady){
			camerasReady = 0;
			m_isMoving = true;
		}
	}

	[RPC]
	public void DamagePlayer( int damage ) {
		if( !GameManager.instance.GOD_MODE ) {
			m_stats.ApplyDamage( damage );
			GUIManager.instance.UpdateHud();

			if( Network.isServer )
				networkView.RPC( "DamagePlayer", RPCMode.Others, damage );
		}
	}
	//checks how many cameras are active on level load
	void CameraCounter(){
		for(int i = 0; i < 4; i++){
			if(cameraManager.IsCameraAssigned(i)){
				camsActive++;
			}
		}
	}
}
