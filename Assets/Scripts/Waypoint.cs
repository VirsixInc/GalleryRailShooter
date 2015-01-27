using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {

	public Waypoint m_next;
	public bool isTimedEvent = false;
	public float eventTime = 10f;

	
	[System.NonSerialized]
	public bool m_checked = false;			// Used for path complete check
	
	void OnDrawGizmosSelected() {
		if( m_next != null ) {
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine( transform.position, m_next.transform.position );
			
		}
	}
}
