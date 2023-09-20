using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    Vector2 m_PreviousPosition;
    Vector2 m_CurrentPosition;
    Vector2 m_NextMovement;

    private int m_StuckCount = 0;
    private float m_Speed;

    void Start()
    {
        m_CurrentPosition = this.transform.position;
        m_PreviousPosition = this.transform.position;
        m_StuckCount = 0;
        if (PlayerCharacter.Instance != null)
            m_Speed = PlayerCharacter.Instance.MoveSpeed;
        else if (GetComponent<PlayerStatus>() != null)
            m_Speed = GetComponent<PlayerStatus>().WalkingSpeed;
    }

    void FixedUpdate()
    {
        m_PreviousPosition = this.transform.position;
        m_CurrentPosition = m_PreviousPosition + m_NextMovement;

        this.transform.position = m_CurrentPosition;
        m_NextMovement = Vector2.zero;
    }

    public void Move(Vector2 movement)
    {
        m_NextMovement += movement; 
    }

    public Vector2 MoveToTargetPoint(Vector2 target, float speed)
    {
        if (speed == 0.0f)
        {
            speed = m_Speed;
        }

        if (Vector2.Distance(target, PlayerCharacter.Instance.BodyTransform.position) > 0.01f)
        {
            Vector2 direction = target - (Vector2)PlayerCharacter.Instance.BodyTransform.position;
            direction.Normalize();

            return direction * speed;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public bool CheckPlayerStuck(Vector2 pos1, Vector2 pos2)
    {
        return false;
    }
}
