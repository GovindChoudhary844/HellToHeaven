using UnityEngine;
using System.Collections;

public class Sensor_HeroKnight : MonoBehaviour {

    private int m_ColCount = 0;

    private float m_DisableTimer;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore colliders attached to the player (preventing false wall detection on ourselves)
        if (other.transform.IsChildOf(transform.parent)) return;
        
        // Ignore triggers (like coins, hitboxes, ropes) so they aren't treated as solid walls
        if (other.isTrigger) return;

        m_ColCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.IsChildOf(transform.parent)) return;
        if (other.isTrigger) return;

        m_ColCount--;
    }

    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}
