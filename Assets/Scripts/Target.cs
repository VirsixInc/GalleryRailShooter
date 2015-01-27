using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	private bool m_hit = false;
	private Animator m_animator;


	void Start () {
		m_animator = GetComponentInChildren<Animator>();
	}

	public void Hit() {
		m_animator.SetTrigger( "Drop" );
		m_hit = true;

		SetColliders (false);
	}

	public void PopUp() {
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
}
