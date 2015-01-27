using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour {

//    public VisionBlockingEnemy[] witchesInWave;
//	public float waveTimer = 0;
//	public bool waveHasStarted = false;
//
//	public bool waveComplete = false;
//	public bool villagersComplete = false;
//
//	public Stats[] allTheStats;
//	public VillagerScript[] villagersInWave;
//	public BasicEnemyScript[] enemiesInWave;
//	public MeleeAttacker[] attackersInWave;
//	//public VisionBlockingEnemy[] witchesInWave;
//
//	public PortalToggle[] portalsInWave;
//	public bool portalsHaveBeenActivated = false;
//
//	public int cameraID = 0;
//	public bool myCameraIsActive = false;
//	public ManagerScript cameraManager;
//	public WaveManager myWaveManager;
//
//	public CameraMove cameraMotor;
//	public bool completionCounted = false;
//	public bool isMovingEvent = false; //mark as true if you dont want to stop at the event
//
//	//on trigger enter
//	//switch bool
//	//call function in update
//
//
//
//	// Use this for initialization
//	void Start () {
//		attackersInWave = this.GetComponentsInChildren<MeleeAttacker>(); 
//		enemiesInWave = this.GetComponentsInChildren<BasicEnemyScript>(true);
//		allTheStats = this.GetComponentsInChildren<Stats>(true);
//		villagersInWave = this.GetComponentsInChildren<VillagerScript>();
//		portalsInWave = this.GetComponentsInChildren<PortalToggle>();
//        //witchesInWave = this.GetComponentsInChildren<VisionBlockingEnemy>();
//
//		cameraMotor = GameObject.Find("CameraMotor").GetComponent<CameraMove>();
//
//		cameraManager = GameObject.Find("CameraManager").GetComponent<ManagerScript>();
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		if(cameraManager.IsCameraAssigned(cameraID)){
//			myCameraIsActive = true;
//			//print (cameraID);
//		}
//		if(myCameraIsActive){
//			if( Network.isServer ) {
//				Wave ();
//				CompletionCheck();
//				CompletionCounter();
//			}
//		}
//
//	}
//
//	void OnTriggerEnter(Collider other){
//
//		if(myCameraIsActive){
//			waveHasStarted = true;
//		}
//
//
//		else{
//			waveHasStarted = false;
//			waveComplete = true;
//			villagersComplete = true;
//			CompletionCheck();
//		}
//
//	}
//
//	public void Wave(){
//
//		if(waveHasStarted){
//            foreach (VisionBlockingEnemy witch in witchesInWave){
//                witch.Activate();
//            }
//			activatePortals();
//			waveTimer += Time.deltaTime;
//			villagerCheck();
//			CompletionCheck();
//		}
//	}
//
//	void CompletionCheck(){
//		int enemiesKilled = 0; //when this number == the total of every enemies in the wave, all enemies are dead
//		//search through all stats scripts childed to the wave
//		//if one is dead count it 
//		foreach(Stats stats in allTheStats){
//			if(!stats.m_isAlive){
//				enemiesKilled++;
//			}
//		}
//		//check that we counted every enemy in the wave
//		//set tcompletion to ttrue
//		if(enemiesKilled == (attackersInWave.Length + enemiesInWave.Length + witchesInWave.Length)){
//			waveComplete = true;
//		}
//	}
//	//did you manage to save any villagers? or are they all dead?
//	//counts through every villager and checks it every time its called
//	void villagerCheck(){
//		int villagersOutOfPlay = 0;
//		foreach(VillagerScript villager in villagersInWave){
//			//if villager is saved, count it as out of play. 
//			if(villager.state == VillagerScript.villagerState.saved){
//				villagersOutOfPlay++;
//			}
//			//if a villager is dead, count it out of play
//			if(villager.state == VillagerScript.villagerState.dead){
//				villagersOutOfPlay++;
//			}
//		}
//		//if all the villagers are out of play (dead or saved)
//		//villagers are complete. 
//		if(villagersOutOfPlay == villagersInWave.Length){
//			villagersComplete = true;
//		}
//	}
//
//
//	//TODO: Add witch to wave manager
//	void witchCheck(){
//
//	}
//
//	//placed inside the wave function. 
//	//makes sure portals are only toggled on once. 
//	void activatePortals(){
//		if(!portalsHaveBeenActivated){
//			foreach(PortalToggle portal in portalsInWave){
//				portal.toggle = true;
//			}
//			portalsHaveBeenActivated = true;
//		}
//	}
//
//	public void deactivatePortals(){
//		foreach(PortalToggle portal in portalsInWave){
//			portal.toggle = true;
//		}
//	}
//
//	void CompletionCounter(){
//		if(villagersComplete && waveComplete && !completionCounted){
//			cameraMotor.camerasReady++;
//			completionCounted = true;
//		}
//	}
//

}
