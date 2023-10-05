using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WildCatch
{
    public class CatchMineral_Manager : CatchBase_Manager
    {
        [Header("矿石图标")]
        public List<Sprite> MineralSprites;

        [Header("滑块转动速度")]
        [SerializeField] private float pointRotateSpeed_easy = 80;
        [SerializeField] private float pointRotateSpeed_normal = 100;
        [SerializeField] private float pointRotateSpeed_hard = 120;
        float roSpeed;

        private Image[] signIcons, hitIcons;
        private Transform pointIcon;

        protected override void Start()
        {
            base.Start();

            signIcons = rollBar.Find("SignIcons").GetComponentsInChildren<Image>();
            hitIcons = rollBar.Find("HitIcons").GetComponentsInChildren<Image>();
            pointIcon = rollBar.Find("PointIcon");
        }

        protected override void StartCatching()
        {
            base.StartCatching();

            // 播放采矿准备动作
            player.SetAnimCatchState(1);
            player.GetComponent<Animator>().Play("CatchMineral_CheckDirection", 0, 0);
        }

        protected override void InitLevel()
        {
            switch (catchDir) {
                case PlayerInput.PlayerDirection.Down:
                case PlayerInput.PlayerDirection.Up:
                    rollBar.position = new Vector3(player.CurrPos.x + 1.6f, player.transform.position.y);
                    break;
                case PlayerInput.PlayerDirection.Left:
                    rollBar.position = new Vector3(player.CurrPos.x - 1.8f, player.transform.position.y);
                    break;
                case PlayerInput.PlayerDirection.Right:
                    rollBar.position = new Vector3(player.CurrPos.x + 1.8f, player.transform.position.y);
                    break;
            }
            rollBar.gameObject.SetActive(true);

            cacherState = ECatcherState.PREPARING;
            pointIcon.localEulerAngles = Vector3.zero;

            var hitRotationZList = new List<int>();
            int rotationZOffset = Random.Range(0, 360);
            switch (currCatchPoint.catchLevel) {
                case ECatchLevel.EASY:
                    roSpeed = pointRotateSpeed_easy;
                    for (int i = 0; i < signIcons.Length; i++) {
                        var roZ = (Random.Range(0, 6) * 60 + rotationZOffset);
                        while (hitRotationZList.Contains(roZ)) {
                            roZ = (Random.Range(0, 6) * 60 + rotationZOffset);
                        }
                        hitRotationZList.Add(roZ);
                        signIcons[i].fillAmount = 0.1f;
                        signIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);

                        hitIcons[i].fillAmount = 0.1f;
                        hitIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);
                        hitIcons[i].gameObject.SetActive(false);
                    }
                    break;
                case ECatchLevel.NORMAL:
                    roSpeed = pointRotateSpeed_normal;
                    for (int i = 0; i < signIcons.Length; i++) {
                        var roZ = (Random.Range(0, 8) * 45 + rotationZOffset);
                        while (hitRotationZList.Contains(roZ)) {
                            roZ = (Random.Range(0, 8) * 45 + rotationZOffset);
                        }
                        hitRotationZList.Add(roZ);
                        signIcons[i].fillAmount = 0.075f;
                        signIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);

                        hitIcons[i].fillAmount = 0.075f;
                        hitIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);
                        hitIcons[i].gameObject.SetActive(false);
                    }
                    break;
                case ECatchLevel.HARD:
                    roSpeed = pointRotateSpeed_hard;
                    for (int i = 0; i < signIcons.Length; i++) {
                        var roZ = (Random.Range(0, 9) * 40 + rotationZOffset);
                        while (hitRotationZList.Contains(roZ)) {
                            roZ = (Random.Range(0, 9) * 40 + rotationZOffset);
                        }
                        hitRotationZList.Add(roZ);
                        signIcons[i].fillAmount = 0.05f;
                        signIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);

                        hitIcons[i].fillAmount = 0.05f;
                        hitIcons[i].transform.localEulerAngles = new Vector3(0, 0, roZ);
                        hitIcons[i].gameObject.SetActive(false);
                    }
                    break;
            }
        }

        protected override void RollIcons()
        {
            pointIcon.localEulerAngles -= new Vector3(0, 0, roSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 检测采矿（准备状态下左键或J键）
        /// </summary>
        protected override void DetectCatch()
        {
            if (playerInput.HasInteractDown()) {
                bool isHit = false;
                for (int i = 0; i < signIcons.Length; i++) {
                    // 单次采矿成功
                    if (IsHitMineSign(pointIcon.localEulerAngles.z, signIcons[i].transform.localEulerAngles.z)) {
                        StartCoroutine(nameof(SuccessCatch), hitIcons[i].gameObject);
                        isHit = true;
                        break;
                    }
                }
                // 采矿失败
                if (!isHit) {
                    StartCoroutine(nameof(FailCatch));
                }
            }
        }

        /// <summary>
        /// 判定单次采矿
        /// </summary>
        private bool IsHitMineSign(float pointRoZ, float signRoZ)
        {
            switch (currCatchPoint.catchLevel) {
                case ECatchLevel.EASY:
                    signRoZ = (signRoZ - 18) % 360;
                    if (Mathf.Abs(pointRoZ - signRoZ) < 23)
                        return true;
                    break;
                case ECatchLevel.NORMAL:
                    signRoZ = (signRoZ - 13.5f) % 360;
                    if (Mathf.Abs(pointRoZ - signRoZ) < 18.5)
                        return true;
                    break;
                case ECatchLevel.HARD:
                    signRoZ = (signRoZ - 9) % 360;
                    if (Mathf.Abs(pointRoZ - signRoZ) < 14)
                        return true;
                    break;
            }
            return false;
        }

        /// <summary>
        /// 判定是否完成三次采矿
        /// </summary>
        private bool IsFinishCatch()
        {
            foreach (var icon in hitIcons) {
                if (!icon.gameObject.activeSelf)
                    return false;
            }
            return true;
        }

        private IEnumerator SuccessCatch(GameObject hitIcon)
        {
            cacherState = ECatcherState.PENDING;
            var preSpeed = roSpeed;
            roSpeed = 0;
            // 播放采矿动画
            player.SetAnimCatchState(3);
            yield return new WaitForSeconds(0.5f);
            hitIcon.SetActive(true);
            roSpeed = preSpeed;

            // 三次采矿成功
            if (IsFinishCatch()) {
                StartCoroutine(nameof(FinishCatch));
            }
            // 恢复准备采矿
            else {
                cacherState = ECatcherState.PREPARING;
                player.SetAnimCatchState(1);
                player.GetComponent<Animator>().Play("CatchMineral_CheckDirection", 0, 0);
            }
        }
        private IEnumerator FinishCatch()
        {
            cacherState = ECatcherState.PENDING;

            yield return new WaitForSeconds(0.3f);
            rollBar.gameObject.SetActive(false);
            currCatchPoint.gameObject.SetActive(false);

            // 出成功捕捉UI
            successUI.SetActive(true);
            yield return new WaitForSeconds(2f);

            successUI.SetActive(false);

            ResetGame();
            player.SetAllowMove(true);

            // TODO: 背包获得矿物
        }

        private IEnumerator FailCatch()
        {
            rollBar.gameObject.SetActive(false);
            // 停顿防止点击响应移动事件
            yield return new WaitForSeconds(0.2f);
            ResetGame();
            player.SetAllowMove(true);
        }

    }
}

