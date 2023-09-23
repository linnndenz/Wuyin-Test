using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatchInsect
{
    public enum CatchLevel { EASY = 0, NORMAL = 1, HARD = 2 }

    public class CatchInsect_Manager : MonoBehaviour
    {
        public static CatchInsect_Manager Instance;

        [Header("捕捉图标（手）的位置偏移")]
        public Vector2 catchBtnOffset = new Vector2(0f, 0f);

        [Header("成功捕捉时，两图标允许的最远距离")]
        public float catchSuccessRance = 25;

        // UI相关
        Transform uiCanvas;
        Button catchBtn;
        Transform rollBar;
        RectTransform backBar, insectIcon, handIcon;
        GameObject successUI;

        // 玩家控制
        PlayerCharacter player;
        PlayerInput playerInput;

        // 捕虫相关
        enum CatcherState { NOT = 0, ARRIVING = 1, PREPARING = 2, PENDING = 3 }
        [SerializeField] CatcherState cacherState;
        [HideInInspector] public CatchInsect_CatchPoint currCatchPoint; // 当前交互的捕虫点

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            player = PlayerCharacter.Instance;
            playerInput = PlayerInput.Instance;

            uiCanvas = transform.GetChild(0);
            catchBtn = uiCanvas.Find("CatchBtn").GetComponent<Button>();
            catchBtn.gameObject.SetActive(false);

            rollBar = uiCanvas.Find("RollBar");
            rollBar.gameObject.SetActive(false);
            backBar = rollBar.Find("BackBar").GetComponent<RectTransform>();
            insectIcon = rollBar.Find("InsectIcon").GetComponent<RectTransform>();
            handIcon = rollBar.Find("HandIcon").GetComponent<RectTransform>();
            successUI = uiCanvas.Find("SuccessUI").gameObject;
            successUI.SetActive(false);

            rollLeft = -backBar.rect.width / 2 + insectIcon.rect.width / 2;
            rollRight = backBar.rect.width / 2 - insectIcon.rect.width / 2;
        }

        private void Update()
        {
            // 进入捕虫状态
            if (cacherState == CatcherState.PREPARING) {
                DetectCatch();
                RollIcons();
            }

            // 防止镜头移动时catchBtn位置偏移
            if (catchBtn.gameObject.activeSelf) {
                catchBtn.transform.position = (Vector2)currCatchPoint.transform.position + catchBtnOffset;
            }
        }

        /// <summary>
        /// 鼠标进入捕虫点时，显示捕虫按钮（手图标）
        /// </summary>
        public void OnMouseEnterCatchPoint(CatchInsect_CatchPoint catchPoint)
        {
            if (cacherState != CatcherState.NOT || !catchPoint) return;
            player.SetAllowInput(false);
            currCatchPoint = catchPoint;
            catchBtn.transform.position = (Vector2)currCatchPoint.transform.position + catchBtnOffset;
            catchBtn.gameObject.SetActive(true);
        }

        /// <summary>
        /// 未交互状态下，鼠标离开捕虫点
        /// </summary>
        public void OnMouseExitCatchPoint(CatchInsect_CatchPoint catchPoint)
        {
            if (cacherState == CatcherState.NOT && catchPoint == currCatchPoint) {
                player.SetAllowInput(true);
                currCatchPoint = null;
                catchBtn.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 点击捕虫按钮
        /// </summary>
        PlayerInput.PlayerDirection catchDir;
        public void OnClickCatchBtn()
        {
            if (cacherState != CatcherState.NOT) return;

            cacherState = CatcherState.ARRIVING;

            catchBtn.gameObject.SetActive(false);

            // 去最近的捕虫点
            float disX = player.CurrPos.x - currCatchPoint.transform.position.x;
            float disY = player.CurrPos.y - currCatchPoint.transform.position.y;

            float offsetX = 0;
            float offsetY = 0;
            if (Mathf.Abs(disX) > Mathf.Abs(disY)) {
                if (disX < 0) {
                    catchDir = PlayerInput.PlayerDirection.Right;
                    offsetX = -2;
                    offsetY = -1.1f;
                }
                else {
                    catchDir = PlayerInput.PlayerDirection.Left;
                    offsetX = 2;
                    offsetY = -1.1f;
                }
            }
            else {
                if (disY < 0) {
                    catchDir = PlayerInput.PlayerDirection.Up;
                    offsetY = -1.1f;
                }
                else {
                    catchDir = PlayerInput.PlayerDirection.Down;
                    offsetY = 0.5f;
                }
            }

            Vector2 desPos = new Vector2(currCatchPoint.transform.position.x + offsetX
                , currCatchPoint.transform.position.y + offsetY);

            //移动至捕虫点
            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            player.SetTargetPoints(new Vector2[] { desPos });
            player.SetMoveFinishAction(StartCatching);
            player.PassInputSignal(2);
            player.SetScriptMoving(true);
        }


        float rollLeft, rollRight;
        int insectDir, handDir;
        float insectMoveSpeed, handMoveSpeed;
        /// <summary>
        /// 角色到达捕虫点后，开始捕虫游戏，开始UI初始化
        /// </summary>
        public void StartCatching()
        {
            player.AnimSetDirection(catchDir);

            // 播放捕虫初始动作
            player.GetComponent<Animator>().Play("CatchInsect_CheckDirection", 0, 0);
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

        public void InitLevel()
        {
            rollBar.position = new Vector3(player.CurrPos.x, player.transform.position.y + 1.8f);
            rollBar.gameObject.SetActive(true);
            insectIcon.anchoredPosition = new Vector2(rollLeft, 0);
            handIcon.localPosition = Vector2.zero;

            insectDir = 1;
            cacherState = CatcherState.PREPARING;

            switch (currCatchPoint.catchLevel) {
                case CatchLevel.EASY:
                    insectMoveSpeed = backBar.rect.width / (3.0f / 2);
                    break;
                case CatchLevel.NORMAL:
                    insectMoveSpeed = backBar.rect.width / (2.0f / 2);
                    break;
                case CatchLevel.HARD:
                    handDir = -1;
                    insectMoveSpeed = backBar.rect.width / (3.0f / 2);
                    handMoveSpeed = backBar.rect.width / (1.5f / 2);
                    handIcon.localPosition = new Vector2(rollRight, 0);
                    break;
            }
        }

        /// <summary>
        /// 滑块滑动
        /// </summary>
        private void RollIcons()
        {
            // 虫移动
            if (insectIcon.anchoredPosition.x > rollRight) insectDir = -1;
            if (insectIcon.anchoredPosition.x < rollLeft) insectDir = 1;
            insectIcon.anchoredPosition += new Vector2(insectDir * insectMoveSpeed * Time.deltaTime, 0);

            // 困难模式，手移动
            if (currCatchPoint.catchLevel == CatchLevel.HARD) {
                if (handIcon.anchoredPosition.x > rollRight) handDir = -1;
                if (handIcon.anchoredPosition.x < rollLeft) handDir = 1;
                handIcon.anchoredPosition += new Vector2(handDir * handMoveSpeed * Time.deltaTime, 0);
            }
        }

        /// <summary>
        /// 检测捕虫（准备状态下左键或J键）
        /// </summary>
        private void DetectCatch()
        {
            if (playerInput.HasInteractInput()) {
                cacherState = CatcherState.PENDING;
                rollBar.gameObject.SetActive(false);

                StartCoroutine(nameof(OnInputToCatch));
            }
        }

        private IEnumerator OnInputToCatch()
        {
            // 网落下动画
            player.SetAnimCatchState(1);
            yield return new WaitForSeconds(0.5f);
            currCatchPoint.gameObject.SetActive(false);

            // 成功捕捉
            if (Vector2.Distance(insectIcon.anchoredPosition, handIcon.anchoredPosition) < catchSuccessRance) {
                StartCoroutine(nameof(SuccessCatch));
            }
            // 捕捉失败
            else {
                StartCoroutine(nameof(FailCatch));
            }
        }

        private IEnumerator SuccessCatch()
        {
            Debug.Log("捕捉成功");

            player.SetAnimCatchState(3);
            yield return new WaitForSeconds(0.5f);

            // 出成功捕捉UI
            successUI.SetActive(true);
            yield return new WaitForSeconds(2f);

            successUI.SetActive(false);

            ResetGame();
            player.SetAllowMove(true);

            // TODO: 背包获得虫子
        }

        private IEnumerator FailCatch()
        {
            Debug.Log("捕捉失败");
            player.SetAnimCatchState(2);
            yield return new WaitForSeconds(0.5f);

            ResetGame();
            player.SetAllowMove(true);
        }

        private void ResetGame()
        {
            player.SetAnimCatchState(0);
            cacherState = CatcherState.NOT;
            currCatchPoint = null;
        }


    }
}
