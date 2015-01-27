using UnityEngine;
using System.Collections;

public class clientScript : MonoBehaviour {
	
	public bool isAssigned = false;
	public ManagerScript.CameraIndex cameraIndex;

  	void OnGUI(){
		if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.NetworkSetup )
    		GUI.Label(new Rect(100,5,100,50), Network.player.ToString());
  	}
}
