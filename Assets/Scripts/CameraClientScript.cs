using UnityEngine;
using System.Collections;

public class CameraClientScript : MonoBehaviour {
	
	public bool isAssigned = false;
	public CameraManager.CameraIndex cameraIndex;

  	void OnGUI(){
		if( GameManager.instance.CurrentMode == (int)GameManager.GameMode.NetworkSetup )
    		GUI.Label(new Rect(100,5,100,50), Network.player.ToString());
  	}
}
