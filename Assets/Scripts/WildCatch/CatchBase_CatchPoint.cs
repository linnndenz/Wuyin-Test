using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildCatch {
    public class CatchBase_CatchPoint : MonoBehaviour
    {
        [Header("素材采集难度")]
        public ECatchLevel catchLevel;

        protected CatchBase_Manager manager;
        protected virtual void Start()
        {
            manager = GetComponentInParent<CatchBase_Manager>();
        }

        protected virtual void OnMouseOver()
        {
            manager.OnMouseEnterCatchPoint(this);
        }

        protected virtual void OnMouseExit()
        {
            manager.OnMouseExitCatchPoint(this);
        }
    }

}
