﻿using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	private bool m_hit = false;
	private Animator m_animator;
	private NetworkView myNetworkView;

	void Start () {
		m_animator = GetComponentInChildren<Animator>();
		myNetworkView = GetComponent<NetworkView>();
		SetColliders( false );
	}

	public void Hit() {
		if( Network.isServer ) {
			m_animator.SetTrigger( "Drop" );
			myNetworkView.RPC( "SendTrigger", RPCMode.Others, "Drop" );
			m_hit = true;
			SetColliders (false);
		}
	}

	public void PopUp() {
		m_animator.SetTrigger( "Rise" );
		myNetworkView.RPC( "SendTrigger", RPCMode.Others, "Drop" );
		SetColliders (true);
	}

	void SetColliders(bool on) {
		Collider[] colliders = GetComponentsInChildren<Collider>();
		
		foreach(Collider col in colliders) {
			col.enabled = on;
		}
	}

	public bool IsHit() {
		return m_hit;
	}

	[RPC]
	void SendTrigger( string name ) {
		m_animator.SetTrigger( name );
	}
}
