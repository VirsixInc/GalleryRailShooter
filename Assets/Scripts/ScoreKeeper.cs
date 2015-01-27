using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
public class ScoreKeeper : MonoBehaviour {

    /// <summary>
    /// attached to manager script
    /// </summary>

    int score = 0;
    // art
    public GUIStyle scoreGUIStyle;

    void OnGUI()
    {
        // top left of screen
        GUILayout.BeginArea(new Rect(32, 32, 200, 200));
        GUILayout.Label("Score: " + score, scoreGUIStyle);
		GUILayout.EndArea();
    }

    public void AddGameStats(int s)
    {
        score += s;
		networkView.RPC( "UpdateScore", RPCMode.Others, score );
    }

	void OnLevelWasLoaded( int level ) {
		score = 0;
	}

	[RPC]
	void UpdateScore( int newScore ) {
		score = newScore;
	}
}
