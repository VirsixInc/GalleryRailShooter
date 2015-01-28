using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider))]
public class Room : Waypoint {

	public Desk[] m_desks;
	private bool m_active = false;

	// Use this for initialization
	void Start () {
		m_stopsPlayer = true;
		m_desks = GetComponentsInChildren<Desk>();
		collider.isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
		if( m_active ) {
			foreach( Desk desk in m_desks ) {
				if( desk.AllTargetsHit() == false )
					return;
			}

			CameraMove.instance.MoveCamAlongSpline();
			m_active = false;
		}
	}

	void OnTriggerEnter( Collider col ) {
		if( col.tag == "Player" ) {
			m_active = true;

			foreach( Desk desk in m_desks ) {
				desk.Activate();
			}
		}
	}
}
