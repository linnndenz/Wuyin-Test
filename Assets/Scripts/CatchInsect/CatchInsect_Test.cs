using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatchInsect
{
    public class CatchInsect_Test : MonoBehaviour
    {
        public Transform catchPoints;
        /// <summary>
        /// 测试用，重置游戏
        /// </summary>
        public void ResetGame()
        {
            for (int i = 0; i < catchPoints.childCount; i++) {
                catchPoints.GetChild(i).gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 测试用，修改虫子等级
        /// </summary>
        public void ChangeCatchLevel(int newLevel)
        {
            if (!CatchInsect_Manager.Instance.currCatchPoint) return;
            CatchInsect_Manager.Instance.currCatchPoint.catchLevel = (CatchLevel)newLevel;
            CatchInsect_Manager.Instance.InitLevel();
            Debug.Log("修改捕虫难度为：" + (CatchLevel)newLevel);
        }
    }
}
