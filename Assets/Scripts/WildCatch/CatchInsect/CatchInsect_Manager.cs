using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WildCatch
{
    public class CatchInsect_Manager : CatchBase_Manager
    {
        [Header("成功捕捉时，两图标允许的最远距离")]
        public float catchSuccessRance = 25;

        RectTransform backBar, insectIcon, handIcon;

        protected override void Start()
        {
            base.Start();
            backBar = rollBar.Find("BackBar").GetComponent<RectTransform>();
            insectIcon = rollBar.Find("InsectIcon").GetComponent<RectTransform>();
            handIcon = rollBar.Find("HandIcon").GetComponent<RectTransform>();

            rollLeft = -backBar.rect.width / 2 + insectIcon.rect.width / 2;
            rollRight = backBar.rect.width / 2 - insectIcon.rect.width / 2;
        }

        protected override void StartCatching()
        {
            base.StartCatching();
            // 播放捕虫初始动作
            player.GetComponent<Animator>().Play("CatchInsect_CheckDirection", 0, 0);
        }

        float rollLeft, rollRight;
        int insectDir, handDir;
        float insectMoveSpeed, handMoveSpeed;
        protected override void InitLevel()
        {
            rollBar.position = new Vector3(player.CurrPos.x, player.transform.position.y + 1.8f);
            rollBar.gameObject.SetActive(true);

            insectIcon.anchoredPosition = new Vector2(rollLeft, 0);
            handIcon.localPosition = Vector2.zero;

            insectDir = 1;
            cacherState = ECatcherState.PREPARING;

            switch (currCatchPoint.catchLevel) {
                case ECatchLevel.EASY:
                    insectMoveSpeed = backBar.rect.width / (3.0f / 2);
                    break;
                case ECatchLevel.NORMAL:
                    insectMoveSpeed = backBar.rect.width / (2.0f / 2);
                    break;
                case ECatchLevel.HARD:
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
        protected override void RollIcons()
        {
            // 虫移动
            if (insectIcon.anchoredPosition.x > rollRight) insectDir = -1;
            if (insectIcon.anchoredPosition.x < rollLeft) insectDir = 1;
            insectIcon.anchoredPosition += new Vector2(insectDir * insectMoveSpeed * Time.deltaTime, 0);

            // 困难模式，手移动
            if (currCatchPoint.catchLevel == ECatchLevel.HARD) {
                if (handIcon.anchoredPosition.x > rollRight) handDir = -1;
                if (handIcon.anchoredPosition.x < rollLeft) handDir = 1;
                handIcon.anchoredPosition += new Vector2(handDir * handMoveSpeed * Time.deltaTime, 0);
            }
        }

        /// <summary>
        /// 检测捕虫（准备状态下左键或J键）
        /// </summary>
        protected override void DetectCatch()
        {
            if (playerInput.HasInteractInput()) {
                cacherState = ECatcherState.PENDING;
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
            player.SetAnimCatchState(2);
            yield return new WaitForSeconds(0.5f);

            ResetGame();
            player.SetAllowMove(true);
        }
    }
}
