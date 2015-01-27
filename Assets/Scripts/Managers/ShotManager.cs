using UnityEngine;
using System.Collections;

public class ShotManager : MonoBehaviour {

	private static ShotManager _instance;

	public int m_damage = 1;
	public GameObject enemyHitParticle;
	public GameObject missedShotParticle;

	private AudioSource m_audio;
	public GameObject mainCamForOSC;
    public GameObject gunFeedback;
	public bool INVERT_X = false;
	public bool INVERT_Y = false;
	
	#region Singleton Initialization
	public static ShotManager instance {
		get {
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<ShotManager>();

			return _instance;
		}
	}

	void Awake() {
		if(_instance == null) {
			//If i am the fist instance, make me the first Singleton
			_instance = this;
		} else {
			//If a Singleton already exists and you find another reference in scene, destroy it
			if(_instance != this)
				Destroy(gameObject);
		}
	}
	#endregion

	void Start () {
		m_audio = ManagerScript.instance.GetComponent<AudioSource>();
		mainCamForOSC = GameObject.Find("0");
	}
	
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			if(GameManager.instance.CurrentMode == (int)GameManager.GameMode.Play) {
				float x = Input.mousePosition.x;
				float y = Input.mousePosition.y;

				if( INVERT_X )
					x = (Screen.width-1) - x;
				if( INVERT_Y )
					y = (Screen.height-1) - y;

				Ray ray = Camera.main.ScreenPointToRay( new Vector3( x, y, Input.mousePosition.z ) );

				if(Network.isServer){
					Shoot(ray.origin, ray.direction);
				}
				else{ 
					networkView.RPC("Shoot", RPCMode.Server, ray.origin, ray.direction);
				}
			} else if ( GameManager.instance.CurrentMode == (int)GameManager.GameMode.Title ) {
				if( Network.isServer ) {
					GameManager.instance.ChangeMode( (int)GameManager.GameMode.Play );
					//CameraMove.instance.MoveCamAlongSpline();
				}
			}
		}
	}

	public void NetworkShoot(float x, float y) {
		// TODO Ghetto fix. Remove this and uncomment bottom when done
		if( INVERT_X )
			x = 1f - x;
		if( INVERT_Y )
			y = 1f - y;

		switch( GameManager.instance.CurrentMode )
		{
		case (int)GameManager.GameMode.Play:
//			if( INVERT_X )
//				x = 1f - x;
//			if( INVERT_Y )
//				y = 1f - y;

			Ray ray = Camera.main.ViewportPointToRay(new Vector3(x, y, 0.0f));
			
			if(Network.isServer)
				Shoot(ray.origin, ray.direction);
			else
				networkView.RPC("Shoot", RPCMode.Server, ray.origin, ray.direction);
			break;

		case (int)GameManager.GameMode.Title:
			if(Network.isServer && y >= 0.25f && y <= 0.75f) {
				GameManager.instance.ChangeMode( (int)GameManager.GameMode.Play );
			}

			break;
		}
	}

	[RPC]
	public void Shoot(Vector3 origin, Vector3 direction) {
		Ray ray = new Ray(origin, direction);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 100f)){
			Debug.DrawLine(ray.origin, hit.point, Color.red, 10f);
            //Network.Instantiate(gunFeedback, hit.point, Quaternion.LookRotation(hit.normal), 0);

			if(hit.transform.tag == "Enemy") {
				m_audio.Play();
				GetHitParticle( hit.point, hit.normal, enemyHitParticle );
				hit.transform.SendMessage("TakeDamage", m_damage);				
			} else {
				GetHitParticle( hit.point, hit.normal, missedShotParticle );
			}
		}
	}

	/// <summary>
	/// Gets the hit particle from the static pool, sets it to the right position, and then gets rid of it when it's done playing.
	/// </summary>
	/// <param name="newPos">New position.</param>
	/// <param name="newRotation">New rotation.</param>
	private void GetHitParticle( Vector3 newPos, Vector3 newRotation, GameObject particlePrefab ) {
		// Get paricle and set position/rotation
		GameObject particle = StaticPool.GetObj( particlePrefab );
		particle.transform.position = newPos;
		particle.transform.rotation = Quaternion.LookRotation( newRotation );

		// Play the particle
		ParticleSystem pSystem = particle.GetComponent<ParticleSystem>();
		pSystem.Play();
		networkView.RPC( "NetworkPlayParticle", RPCMode.Others, pSystem.networkView.viewID );
		StartCoroutine( "ResetParticle", pSystem );
	}

	IEnumerator ResetParticle( ParticleSystem particleSystem ) {
		while( true ) {
			yield return new WaitForSeconds(0.05f);

			if(!particleSystem.IsAlive(true))
			{
				particleSystem.GetComponent<StaticPoolActive>().m_activeInScene = false;
				break;
			}
		}
	}

	[RPC]
	void NetworkPlayParticle( NetworkViewID id ) {
		NetworkView.Find( id ).GetComponent<ParticleSystem>().Play();
	}
}
