using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    public static PlayerInput Instance
    {
        get { return s_Instance; }
    }
    protected static PlayerInput s_Instance;

    public bool HaveControl { get { return m_HaveControl; } }

    public InputButton Pause = new InputButton(KeyCode.Escape, XboxControllerButtons.Menu);
    public InputButton Interact = new InputButton(KeyCode.J, XboxControllerButtons.Y);
    public InputButton Left = new InputButton(KeyCode.A);
    public InputButton Right = new InputButton(KeyCode.D);
    public InputButton Up = new InputButton(KeyCode.W);
    public InputButton Down = new InputButton(KeyCode.S);
    public InputAxis Horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal);
    public InputAxis Vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical);

    public enum PlayerDirection
    {
        Down,
        Up,
        Left,
        Right
    }

    public Dictionary<string, PlayerDirection> Directions = new Dictionary<string, PlayerDirection>() 
    {
        { "down", PlayerDirection.Down },
        { "up" , PlayerDirection.Up },
        { "left" , PlayerDirection.Left },
        { "right" , PlayerDirection.Right },
        { "" , PlayerDirection.Down }
    };

    protected bool m_HaveControl = true;
    protected bool m_DebugMenuIsOpen = false;

    private int m_CurInputType = -1;

    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");
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

    void FixedUpdate()
    {
        m_CurInputType = PlayerCharacter.Instance.GetInputType();

        switch (m_CurInputType)
        {
            // No input
            case -1:
                break;
            // Keyboard
            case 0:
                {
                    if (HasKeyInput() == false)
                    {
                        PlayerCharacter.Instance.PassInputSignal(-1);
                        PlayerCharacter.Instance.SetPlayerMoveVector(Vector2.zero);
                    }
                }
                break;
            case 1:
                {
                    if (HasMouseInput() == false)
                    {
                        PlayerCharacter.Instance.PassInputSignal(-1);
                    }
                }
                break;
            default:
                break;
        }

        if (PlayerCharacter.Instance.IsScriptMoving() == false)
        {
            if (HasKeyInput() == true)
            {
                // Keyboard
                PlayerCharacter.Instance.PassInputSignal(0);
            }
            if (HasMouseInput())
            {
                // Mouse
                PlayerCharacter.Instance.PassInputSignal(1);
            }
        }
    }

    public bool HasKeyUp()
    {
        if (Left.Up ||
            Right.Up ||
            Up.Up ||
            Down.Up)
            return true;

        return false;
    }

    public bool HasKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A))
            return true;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W))
            return true;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S))
            return true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
            return true;
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKey(KeyCode.J))
            return true;

        return false;
    }

    public bool HasMouseInput()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(0))
            return true;

        return false;
    }

    public bool HasInteractInput()
    {
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKey(KeyCode.J))
        {
            return true;
        }
        if (Input.GetMouseButtonUp(0))
            return true;

        return false;
    }

    public bool ProceedDialogue()
    {
        if (Input.GetKeyUp(KeyCode.J))
        {
            return true;
        }
        if (Input.GetMouseButtonUp(0))
            return true;

        return false;
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        Pause.Get(fixedUpdateHappened, inputType);
        Interact.Get(fixedUpdateHappened, inputType);
        Left.Get(fixedUpdateHappened, inputType);
        Right.Get(fixedUpdateHappened, inputType);
        Up.Get(fixedUpdateHappened, inputType);
        Down.Get(fixedUpdateHappened, inputType);
        Horizontal.Get(inputType);
        Vertical.Get(inputType);

        if (Input.GetKeyDown(KeyCode.F12))
        {
            m_DebugMenuIsOpen = !m_DebugMenuIsOpen;
        }
    }

    public override void GainControl()
    {
        m_HaveControl = true;

        GainControl(Pause);
        GainControl(Interact);
        GainControl(Horizontal);
        GainControl(Vertical);
        GainControl(Left);
        GainControl(Right);
        GainControl(Up);
        GainControl(Down);
    }

    public override void ReleaseControl(bool resetValues = true)
    {
        m_HaveControl = false;

        ReleaseControl(Pause, resetValues);
        ReleaseControl(Interact, resetValues);
        ReleaseControl(Horizontal, resetValues);
        ReleaseControl(Vertical, resetValues);
        ReleaseControl(Left, resetValues);
        ReleaseControl(Right, resetValues);
        ReleaseControl(Up, resetValues);
        ReleaseControl(Down, resetValues);
    }

    public void GainControlInteract()
    {
        GainControl(Interact);
    }
}
