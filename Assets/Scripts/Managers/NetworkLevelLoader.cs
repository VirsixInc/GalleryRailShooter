using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkLevelLoader : MonoBehaviour {
	
    public GUISkin m_guiSkin;

    public string[] m_supportedNetworkLevels;
    public int m_disconnectedLevel = 0;

    private string m_lastLoadedLevel;

    int lastLevelPrefix = 0;
    bool m_prevStart = false;

    void Awake()
    {
        DontDestroyOnLoad(this);
        networkView.group = 1;
    }
	
	// Update is called once per frame
	void Update () {
	}

	public int GetLevelPrefix()
	{
		return lastLevelPrefix;
	}


    void OnGUI()
    {
        if (Network.peerType != NetworkPeerType.Disconnected)
        {
            if ( Network.isServer )
            {                
                if ( GameManager.instance.CurrentMode == (int)GameManager.GameMode.CameraSetup )
                {
                    if ( GUILayout.Button("Go!"))
                    {
                        Network.RemoveRPCsInGroup(0);
                        Network.RemoveRPCsInGroup(1);
                        //load the very first game level
                        networkView.RPC("LoadLevel", RPCMode.AllBuffered, 1, lastLevelPrefix + 1);
                    }
                }
            }
        }
    }

	public void TransitionToLevel( int newLevel ) {
		Network.RemoveRPCsInGroup(0);
		Network.RemoveRPCsInGroup(1);
		
		networkView.RPC("LoadLevel", RPCMode.AllBuffered, newLevel, lastLevelPrefix + 1);
	}

    [RPC]
    IEnumerator LoadLevel(int level, int levelPrefix)
    {
        lastLevelPrefix = levelPrefix;

		//Set the name of the last level loaded so we can keep track of it
		m_lastLoadedLevel = Application.loadedLevelName;

        Network.SetSendingEnabled(0, false);
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);

		//load the new level
        Application.LoadLevel(level);
        
        yield return null;
        yield return null;
        Network.isMessageQueueRunning = true;
        Network.SetSendingEnabled(0, true);

        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
    }


    void OnDisconnectedFromServer()
    {
		if( Network.isClient ) {
			GameManager.instance.ChangeMode( (int)GameManager.GameMode.NetworkSetup );
	        Application.LoadLevel(m_disconnectedLevel);
		}
    }
}
