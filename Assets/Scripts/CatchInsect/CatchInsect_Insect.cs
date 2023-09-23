using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CatchInsect
{
    public class CatchInsect_Insect : MonoBehaviour
    {
        [Header("虫浮动速度")]
        public float floatSpeed = 0.5f;

        [Header("虫浮动范围")]
        public float floatRangeX = 0.4f;
        public float floatRangeY = 0.4f;

        [Header("浮动时，变换方向时间间隔")]
        public float minFloatInterval = 2f;
        public float maxFloatInterval = 3f;
        float timer;

        private Vector3 desPos = new Vector3();
        void Update()
        {
            // 变换方向计时
            if (timer <= 0) {
                timer = Random.Range(minFloatInterval, maxFloatInterval);
                desPos.x = Random.Range(-floatRangeX, floatRangeX);
                desPos.y = Random.Range(-floatRangeX, floatRangeY);
            }
            timer -= Time.deltaTime;

            // 昆虫移动
            transform.localPosition += (desPos - transform.localPosition) * Time.deltaTime * floatSpeed;
        }
    }
}
