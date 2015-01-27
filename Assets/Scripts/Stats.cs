using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {

    // attach script to all enemy types

    // health variables
    // current health should be private/ access through function GetHealth()
    public int m_maxHealth = 3;
	//[System.NonSerialized]
    public int m_currHealth;

    //speed variables
    public float m_speed = 1.0f;

    // timer variables
    public float m_attackTimer = 0;
    public float m_attackCooldown = 1f;
    public float m_attackDistance = 5.0f;

	public bool m_isAlive = true;

	// Use this for initialization
	void Start () {
        m_currHealth = m_maxHealth;
        m_attackTimer = m_attackCooldown;
	}

    public void ApplyDamage(int damage)
    {
        m_currHealth -= damage;

        if (m_currHealth <= 0)
            m_isAlive = false;
    }

    public void Heal( int healAmount )
    {
		m_currHealth += healAmount;
    }
	

	public void Reset() {
		m_speed = 1.0f;
		m_currHealth = m_maxHealth;
	}
}
