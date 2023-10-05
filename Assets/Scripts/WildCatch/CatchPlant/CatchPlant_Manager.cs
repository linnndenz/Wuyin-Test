using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WildCatch
{
    public class CatchPlant_Manager : CatchBase_Manager
    {
        [Header("花草蘑菇图标")]
        public List<Sprite> PlantSprites;
        public List<Sprite> MushroomSprites;
        [Header("滑块下降速度")]
        [SerializeField] float upSpeed = 2000;
        [SerializeField] float downSpeed_easy = 10;
        [SerializeField] float downSpeed_normal = 25;
        [SerializeField] float downSpeed_hard = 40;
        RectTransform backBar, pointIcon;
        GameObject operateTip;

        protected override void Start()
        {
            base.Start();

            backBar = rollBar.Find("BackBar").GetComponent<RectTransform>();
            pointIcon = rollBar.Find("PointIcon").GetComponent<RectTransform>();
            operateTip = rollBar.Find("OperateTip").gameObject;

            rollTop = 76;
            rollBottom = -76;
        }

        protected override void StartCatching()
        {
            base.StartCatching();

            // 播放采集准备动作
            player.SetAnimCatchState(1);
            player.GetComponent<Animator>().Play("CatchPlant_CheckDirection", 0, 0);
        }

        float rollTop, rollBottom;
        float downSpeed;
        protected override void InitLevel()
        {
            switch (catchDir) {
                case PlayerInput.PlayerDirection.Down:
                case PlayerInput.PlayerDirection.Up:
                    rollBar.position = new Vector3(player.CurrPos.x + 1.1f, player.transform.position.y);
                    break;
                case PlayerInput.PlayerDirection.Left:
                    rollBar.position = new Vector3(player.CurrPos.x - 1.5f, player.transform.position.y);
                    break;
                case PlayerInput.PlayerDirection.Right:
                    rollBar.position = new Vector3(player.CurrPos.x + 1.5f, player.transform.position.y);
                    break;
            }
            rollBar.gameObject.SetActive(true);

            // 设置滑块起始位置
            pointIcon.anchoredPosition = new Vector2(0, rollBottom + (rollTop - rollBottom) / 3);

            cacherState = ECatcherState.PREPARING;

            switch (currCatchPoint.catchLevel) {
                case ECatchLevel.EASY:
                    downSpeed = downSpeed_easy;
                    break;
                case ECatchLevel.NORMAL:
                    downSpeed = downSpeed_normal;
                    break;
                case ECatchLevel.HARD:
                    downSpeed = downSpeed_hard;
                    break;
            }

            operateTip.SetActive(true);
        }


        protected override void RollIcons()
        {
            // 滑块自然下降
            pointIcon.anchoredPosition -= new Vector2(0, downSpeed * Time.deltaTime);

            // 采集失败
            if (pointIcon.anchoredPosition.y <= rollBottom) {
                StartCoroutine(nameof(FailCatch));
            }
        }

        /// <summary>
        /// 检测采摘（准备状态下左键或J键）
        /// </summary>
        protected override void DetectCatch()
        {
            // 点击滑块上升
            if (playerInput.HasInteractDown()) {
                pointIcon.anchoredPosition += new Vector2(0, upSpeed * Time.deltaTime);

                // 采集成功
                if (pointIcon.anchoredPosition.y >= rollTop) {
                    StartCoroutine(nameof(SuccessCatch));
                }
            }
        }


        private IEnumerator SuccessCatch()
        {
            operateTip.SetActive(false);
            rollBar.gameObject.SetActive(false);

            cacherState = ECatcherState.PENDING;

            player.SetAnimCatchState(3);
            yield return new WaitForSeconds(0.3f);
            currCatchPoint.gameObject.SetActive(false);

            // 出成功捕捉UI
            successUI.SetActive(true);
            yield return new WaitForSeconds(2f);

            successUI.SetActive(false);

            ResetGame();
            player.SetAllowMove(true);

            // TODO: 背包获得花草
        }

        private IEnumerator FailCatch()
        {
            cacherState = ECatcherState.PENDING;
            operateTip.SetActive(false);
            rollBar.gameObject.SetActive(false);
            // 停顿防止点击响应移动事件
            yield return new WaitForSeconds(0.2f);
            ResetGame();
            player.SetAllowMove(true);
        }

    }

}
