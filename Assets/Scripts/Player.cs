using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private Stats m_stats;

	public void TakeDamage(int damage) {
		m_stats.ApplyDamage(damage);
	}
}
