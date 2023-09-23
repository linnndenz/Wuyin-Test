using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CatchInsect
{
    public class CatchInsect_CatchPoint : MonoBehaviour
    {
        [Header("捕虫等级")]
        public CatchLevel catchLevel;

        CatchInsect_Manager manager;
        private void Start()
        {
            manager = GetComponentInParent<CatchInsect_Manager>();
        }

        void OnMouseEnter()
        {
            manager.OnMouseEnterCatchPoint(this);
        }

        void OnMouseExit()
        {
            manager.OnMouseExitCatchPoint(this);
        }


    }
}