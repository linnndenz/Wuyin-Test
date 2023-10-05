using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WildCatch
{
    public enum ECatchLevel { EASY = 0, NORMAL = 1, HARD = 2 }
    public class CatchBase_Manager : MonoBehaviour
    {

        [Header("角色站位偏移")]
        [SerializeField] private Vector2 offsetUp;
        [SerializeField] private Vector2 offsetDown;
        [SerializeField] private Vector2 offsetLeft;
        [SerializeField] private Vector2 offsetRight;

        // UI相关
        protected Transform uiCanvas;
        protected Button catchBtn;
        protected Transform rollBar;
        protected GameObject successUI;

        // 玩家控制
        protected PlayerCharacter player;
        protected PlayerInput playerInput;

        // 捕捉相关
        protected enum ECatcherState { NOT = 0, ARRIVING = 1, PREPARING = 2, PENDING = 3 }
        protected ECatcherState cacherState;

        [HideInInspector] public CatchBase_CatchPoint currCatchPoint; // 当前交互的捕捉点

        protected virtual void Start()
        {
            player = PlayerCharacter.Instance;
            playerInput = PlayerInput.Instance;

            uiCanvas = transform.GetChild(0);
            catchBtn = uiCanvas.Find("CatchBtn").GetComponent<Button>();
            catchBtn.gameObject.SetActive(false);

            rollBar = uiCanvas.Find("RollBar");
            rollBar.gameObject.SetActive(false);

            successUI = uiCanvas.Find("SuccessUI").gameObject;
            successUI.SetActive(false);
        }

        private void Update()
        {
            // 进入捕虫状态
            if (cacherState == ECatcherState.PREPARING) {
                DetectCatch();
                RollIcons();
            }

            // 防止镜头移动时catchBtn位置偏移
            if (catchBtn.gameObject.activeSelf) {
                catchBtn.transform.position = (Vector2)currCatchPoint.transform.position;
            }
        }

        /// <summary>
        /// 鼠标进入捕捉点时，显示捕捉按钮
        /// </summary>
        public void OnMouseEnterCatchPoint(CatchBase_CatchPoint catchPoint)
        {
            if (!player.AllowMove() || player.IsScriptMoving() || cacherState != ECatcherState.NOT || catchPoint == currCatchPoint) return;
            player.SetAllowInput(false);
            currCatchPoint = catchPoint;
            catchBtn.transform.position = (Vector2)currCatchPoint.transform.position;
            catchBtn.gameObject.SetActive(true);
        }

        /// <summary>
        /// 未交互状态下，鼠标离开捕捉点，关闭捕捉按钮
        /// </summary>
        public void OnMouseExitCatchPoint(CatchBase_CatchPoint catchPoint)
        {
            if (cacherState == ECatcherState.NOT && catchPoint == currCatchPoint) {
                player.SetAllowInput(true);
                currCatchPoint = null;
                catchBtn.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// 点击捕捉按钮
        /// </summary>
        protected PlayerInput.PlayerDirection catchDir;
        public void OnClickCatchBtn()
        {
            if (cacherState != ECatcherState.NOT) return;

            cacherState = ECatcherState.ARRIVING;

            catchBtn.gameObject.SetActive(false);

            // 去最近的捕虫点
            float disX = player.CurrPos.x - currCatchPoint.transform.position.x;
            float disY = player.CurrPos.y - currCatchPoint.transform.position.y;

            Vector2 playerPosOffset;
            if (Mathf.Abs(disX) > Mathf.Abs(disY)) {
                if (disX < 0) {
                    catchDir = PlayerInput.PlayerDirection.Right;
                    playerPosOffset = offsetRight;
                }
                else {
                    catchDir = PlayerInput.PlayerDirection.Left;
                    playerPosOffset = offsetLeft;
                }
            }
            else {
                if (disY < 0) {
                    catchDir = PlayerInput.PlayerDirection.Up;
                    playerPosOffset = offsetUp;
                }
                else {
                    catchDir = PlayerInput.PlayerDirection.Down;
                    playerPosOffset = offsetDown;
                }
            }

            Vector2 desPos = new Vector2(currCatchPoint.transform.position.x + playerPosOffset.x
                , currCatchPoint.transform.position.y + playerPosOffset.y);

            //移动至捕虫点
            player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            player.SetTargetPoints(new Vector2[] { desPos });
            player.SetMoveFinishAction(StartCatching);
            player.PassInputSignal(2);
            player.SetScriptMoving(true);
        }

        /// <summary>
        /// 角色到达捕虫点后，开始捕虫游戏，开始UI初始化
        /// </summary>
        protected virtual void StartCatching()
        {
            player.AnimSetDirection(catchDir);

            // 禁止玩家移动
            player.SetAllowMove(false);

            StartCoroutine(nameof(InitCatching));
        }

        private IEnumerator InitCatching()
        {
            yield return new WaitForSeconds(0.3f);

            // 初始化捕虫条玩法
            InitLevel();
        }
        protected virtual void InitLevel() { }
        protected virtual void RollIcons() { }
        protected virtual void DetectCatch() { }

        protected void ResetGame()
        {
            player.SetAnimCatchState(0);
            cacherState = ECatcherState.NOT;
            currCatchPoint = null;
        }

    }


}

