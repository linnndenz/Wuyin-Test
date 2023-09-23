using System.Collections;
using System;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public static PlayerCharacter Instance
    {
        get { return s_Instance; }
    }
    protected static PlayerCharacter s_Instance;

    public Camera MainCamera;

    // Exposed Editable
    public Transform FootTransform;
    [HideInInspector]public Transform BodyTransform;
    public float MoveSpeed;
    public GameObject BubbleText;

    // Status
    private PlayerStatus m_PlayerStatus;

    // Movement
    protected CharacterController m_CharacterController;
    protected ScriptedMove m_ScriptedMove;
    private Vector2 m_MoveVector;
    private int m_TargetPointIdx = 0;
    private Vector2[] m_TargetPoints;
    private Vector2[] m_EnterScenePoints;
    private Vector2 m_TargetPoint;
    private Vector2 m_PreviousPosition;
    private Vector2 m_CharHeight;

    public Vector2 CurrPos => FootTransform.position;

    // Input signal
    private int m_InputType = -1;  // 0: Keyboard; 1: Mouse; 2: Scirpt
    private Action m_MoveFinishAction;
    private int m_Climbing = -1; // 1: up 2: down

    // Interaction
    private GameObject m_ActiveInteractObj;

    // ChatBubble
    private GameObject m_BubbleObj;

    // Animation
    private Animator m_Animator;
    protected readonly int m_AnimWalking = Animator.StringToHash("walking");
    protected readonly int m_AnimClimbing = Animator.StringToHash("climbing");
    protected readonly int m_AnimClimbIdx = Animator.StringToHash("climb");
    protected readonly int m_AnimDirection = Animator.StringToHash("direction");
    protected readonly int m_AnimCatchState = Animator.StringToHash("catchState");
    private string m_ForceAnimation = "";

    // Flags
    private bool m_AllowMove = true;
    private bool m_AllowInput = true;
    private bool m_IsSriptMoving = false;
    private bool m_MovingToTarget = false;
    private bool m_NeedRefreshEvent = false;

    public Vector2 testMapSize = new Vector2(105.94f, 88.97f);

    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");

        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_PreviousPosition = this.transform.position;

        m_MoveFinishAction = null;

        m_PlayerStatus = GetComponent<PlayerStatus>();
        MoveSpeed = m_PlayerStatus.WalkingSpeed;
        BodyTransform = transform;
        m_CharHeight = new Vector2(0, BodyTransform.position.y - FootTransform.position.y);

        ResetCharacter();
    }

    void OnEnable()
    {
        if (s_Instance == null)
            s_Instance = this;
        else if (s_Instance != this)
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");
    }

    void OnDisable()
    {
        s_Instance = null;
    }

    void Update()
    {
        if (AllowMove() == true)
        {
            if (IsMovingToTarget() == true)
            {
                HandleMoveToTarget();
            }

            if (AllowInput() == true &&
                IsScriptMoving() == false)
            {
                switch (m_InputType)
                {
                    // Keyboard
                    case 0:
                        {
                            if (PlayerInput.Instance.HasInteractInput() == true)
                            {
                                if (m_ActiveInteractObj != null)
                                {
                                    var interaction = m_ActiveInteractObj.GetComponent<ObjectInteraction>();
                                    if (interaction != null)
                                    {
                                        if (interaction.Type == "move")
                                        {
                                            interaction.StartMove();
                                        }
                                    }
                                }
                            }
                            else if (PlayerInput.Instance.HasKeyInput() == true)
                            {
                                SetMovingToTarget(false);
                                // Keyboard Moving
                                m_MoveVector.x = PlayerInput.Instance.Horizontal.Value;
                                m_MoveVector.y = PlayerInput.Instance.Vertical.Value;
                                m_MoveVector.Normalize();
                                m_MoveVector = m_MoveVector * MoveSpeed;
                            }
                        }
                        break;
                    // Mouse
                    case 1:
                        {
                            if (m_ActiveInteractObj != null)
                            {
                                var interaction = m_ActiveInteractObj.GetComponent<ObjectInteraction>();
                                if (interaction != null)
                                {
                                    if (interaction.Type == "move")
                                    {
                                        interaction.StartMove();
                                    }
                                }
                            }

                            // Mouse Moving
                            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                            Vector2 lastClickedPos = MainCamera.ScreenToWorldPoint(mousePos);
                            // Move the clicked point to foot
                            lastClickedPos += (Vector2)(this.transform.position - FootTransform.position);

                            MoveToTargetPoint(lastClickedPos);
                        }
                        break;
                    // Script
                    case 2:
                        {
                            PassInputSignal(-1);
                            SetScriptMoving(true);
                            m_TargetPointIdx = 0;
                            HandleScriptMoving();
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (IsScriptMoving() == true)
            {
                HandleScriptMoving();

                var dis = m_TargetPoints[m_TargetPointIdx] - (Vector2)FootTransform.position;
                if (m_MoveVector.magnitude < 0.01f &&
                    dis.magnitude < 0.01f)
                {               
                    if (m_TargetPointIdx < m_TargetPoints.Length - 1)
                    {
                        m_TargetPointIdx++;
                    }
                    // Reach target point
                    else if (m_TargetPointIdx == m_TargetPoints.Length - 1)
                    {
                        // Reach the last target
                        HandleReachLastTarget();
                    }
                    
                }
            }

            m_PreviousPosition = this.transform.position;
            UpdateCharacterAnimation(m_MoveVector);
            m_CharacterController.Move(m_MoveVector * Time.deltaTime);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Event":
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Event":
                break;
            default:
                break;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Event":
                break;
            default:
                break;
        }
    }

    public void SetNeedRefreshEvent(bool flag)
    {
        m_NeedRefreshEvent = flag;
    }

    public void PassInputSignal(int type)
    {
        m_InputType = type;
    }

    public int GetInputType()
    {
        return m_InputType;
    }

    public void SetPlayerMoveVector(Vector2 move)
    {
        m_MoveVector = move;
    }

    public void SetPlayerPosition(float x, float y)
    {
        var pos = new Vector2(x, y - m_CharHeight.y);
    }

    public void ResetCharacter()
    {
        m_AllowMove = true;
        m_AllowInput = true;
        m_IsSriptMoving = false;
        m_MovingToTarget = false;
    }

    public void SetActiveInteractObject(GameObject obj)
    {
        m_ActiveInteractObj= obj;
    }

    public Vector3 GetCharacterHeight()
    {
        return new Vector3(0, BodyTransform.position.y - FootTransform.position.y, 0);
    }

    // Moving
    // ----------------------
    bool IsMoving()
    {
        if (AllowMove() == false)
            return false;

        if (IsScriptMoving() == true)
            return true;
        if (IsMovingToTarget() == true)
            return true;

        return m_MoveVector.magnitude > 0.01f;
    }

    public void MoveToTargetPoint(Vector2 target)
    {
        m_TargetPoint = target;
        m_MoveVector = m_CharacterController.MoveToTargetPoint(target, MoveSpeed);

        SetMovingToTarget(true); 
    }

    void HandleMoveToTarget()
    {
        m_MoveVector = m_CharacterController.MoveToTargetPoint(m_TargetPoint, MoveSpeed);

        float dis = Vector2.Distance(this.transform.position, m_TargetPoint);
        if (dis < 0.1f)
        {
            this.transform.position = m_TargetPoint;
            SetPlayerMoveVector(Vector2.zero);

            SetMovingToTarget(false);
        }
        else
        {
            if (m_CharacterController.CheckPlayerStuck(m_PreviousPosition, this.transform.position))
            {
                SetPlayerMoveVector(Vector2.zero);
                SetMovingToTarget(false);
            }
        }
    }

    public void SetTargetPoints(Vector2[] targets)
    {
        m_TargetPoints = targets;
    }

    public void SetNewScenePoints(Vector2[] targets)
    {
        m_EnterScenePoints = targets;
    }

    void HandleScriptMoving()
    {
        if (m_TargetPoints.Length == 0)
        {
            Debug.LogError("There is no target points set!");
            return;
        }
        if (m_TargetPointIdx >= m_TargetPoints.Length)
        {
            Debug.LogError(string.Format("{0} has exceeded TargetPoint size {1}!", m_TargetPointIdx, m_TargetPoints.Length));
            return;
        }

        MoveToTargetPoint(m_TargetPoints[m_TargetPointIdx] + (Vector2)GetCharacterHeight());
    }

    void UpdateCharacterAnimation(Vector2 move)
    {
        AnimCharacterDirection(move);

        if (IsMoving() == false)
        {
            // Stop
            AnimCharacterWalking(false);
            m_Climbing = 0;
            AnimCharacterClimbing(0);
        }
        else
        {
            if (m_Climbing > 0)
            {
                AnimCharacterWalking(false);
                AnimCharacterClimbing(m_Climbing);
            }
            else
            {
                AnimCharacterClimbing(0);
                AnimCharacterWalking(true);
            }
        }
    }

    void HandleReachLastTarget()
    {   
        SetScriptMoving(false);
        SetMovingToTarget(false);
        PassInputSignal(-1);
        m_TargetPointIdx = 0;

        AnimCharacterWalking(false);
    }

    public IEnumerator WaitForScriptMove()
    {
        SetAllowInput(false);
        yield return new WaitUntil(() => IsScriptMoving() == false);

        if (m_MoveFinishAction != null)
        {
            m_MoveFinishAction.Invoke();
            // clean
            m_ScriptedMove = null;
        }
        SetAllowInput(true);
    }
    //=======================


    // Animation
    // ----------------------
    public void SetForceAnimation(string anim)
    {
        m_ForceAnimation = anim.ToLower();
    }

    public void AnimSetDirection(PlayerInput.PlayerDirection dir)
    {
        m_Animator.SetInteger(m_AnimDirection, (int)dir);
    }

    public PlayerInput.PlayerDirection GetDirectionBetweenPoints(Vector2 source, Vector2 target)
    {
        Vector2 dir = target - source;
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            if (dir.y > 0)
            {
                // Up
                return PlayerInput.PlayerDirection.Up;
            }
            else
            {
                // Down
                return PlayerInput.PlayerDirection.Down;
            }
        }
        else
        {
            if (dir.x > 0)
            {
                // Right
                return PlayerInput.PlayerDirection.Right;
            }
            else
            {
                // Left
                return PlayerInput.PlayerDirection.Left;
            }
        }
    }

    public void SetAnimClimbing(string dir)
    {
        if (dir.ToLower() == "up")
        {
            m_Climbing = 1;
        }
        else if (dir.ToLower() == "down")
        {
            m_Climbing = 2;
        }
        else
        {
            m_Climbing = 0;
        }
    }

    void AnimCharacterClimbing(int climb)
    {
        // 1 - up; 2 - down
        if (climb >= 0 && climb <= 2)
        {
            if (climb > 0)
                m_Animator.SetBool(m_AnimClimbing, true);
            else
                m_Animator.SetBool(m_AnimClimbing, false);

            m_Animator.SetInteger(m_AnimClimbIdx, climb);
        }
    }

    public void AnimCharacterWalking(bool flag)
    {
        if (flag == true)
        {
            // Walking
            m_Animator.SetBool(m_AnimWalking, true);
        }
        else
        {
            // Standing
            m_Animator.SetBool(m_AnimWalking, false);
        }
    }

    void AnimCharacterDirection(Vector2 move)
    {
        if (m_ForceAnimation.Length > 0 && m_ForceAnimation.StartsWith("walk"))
        {
            if (m_ForceAnimation.EndsWith("up"))
            {
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Up);
            }
            else if (m_ForceAnimation.EndsWith("down"))
            {
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Down);
            }
            else if (m_ForceAnimation.EndsWith("left"))
            {
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Left);
            }
            else if (m_ForceAnimation.EndsWith("right"))
            {
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Right);
            }
            return;
        }

        if (Mathf.Abs(move.y) > Mathf.Abs(move.x))
        {
            if (move.y > 0)
            {
                // Up
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Up);
            }
            else if (move.y < 0)
            {
                // Down
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Down);
            }
        }
        else
        {
            if (move.x > 0)
            {
                // Right
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Right);
            }
            else if (move.x < 0)
            {
                // Left
                m_Animator.SetInteger(m_AnimDirection, (int)PlayerInput.PlayerDirection.Left);
            }
        }
    }

    public void SetAnimCatchState(int s)
    {
        m_Animator.SetInteger(m_AnimCatchState, s);
    }

    public void AttachTextToBubble()
    {
        BubbleText.transform.position = this.transform.Find("TextPos").position;
    }

    public void ResetTextPosition()
    {
        if (BubbleText != null)
        {
            BubbleText.transform.position = new Vector2(-100, 0);
        }
    }

    void HandleChangeLayer(GameObject target, bool hide)
    {
        SpriteRenderer targetSprite = target.GetComponent<SpriteRenderer>();
        if (targetSprite)
        {
            if (hide == true)
            {
                if (targetSprite.sortingLayerName == "CoveredByChar")
                {
                    targetSprite.sortingLayerName = "CoverChar";
                }
                target.GetComponent<CombineChangeLayer>().UpdateCombinedObjecsLayer();
            }
            else
            {
                if (targetSprite.sortingLayerName == "CoverChar")
                {
                    targetSprite.sortingLayerName = "CoveredByChar";
                }
                target.GetComponent<CombineChangeLayer>().UpdateCombinedObjecsLayer();
            }
        }
    }
    
    public void SetMoveFinishAction(Action moveAction)
    {
        m_MoveFinishAction = moveAction;
    }
    //=======================

    // Status
    // ----------------------
    public bool AllowMove()
    {
        return m_AllowMove;
    }

    public void SetAllowMove(bool flag)
    {
        var content = flag ? "True" : "False";
        Debug.Log("Set Allow Move: " + content);
        m_AllowMove = flag;

        if (flag == false)
        {
            m_MoveVector = Vector2.zero;
            SetMovingToTarget(false);
            SetScriptMoving(false);
            PassInputSignal(-1);

            AnimCharacterWalking(false);
        }
    }

    bool AllowInput()
    {
        if (m_AllowInput == false)
            return false;

        return true;
    }

    public void SetAllowInput(bool flag)
    {
        m_AllowInput = flag;
    }

    public bool IsScriptMoving()
    {
        return m_IsSriptMoving;
    }

    public void SetScriptMoving(bool flag)
    {
        m_IsSriptMoving = flag;
        if (flag == true)
        {
            StartCoroutine(WaitForScriptMove());
        }
    }

    bool IsMovingToTarget()
    {
        return m_MovingToTarget;
    }

    void SetMovingToTarget(bool flag)
    {
        m_MovingToTarget = flag;
    }
    //=======================
}
