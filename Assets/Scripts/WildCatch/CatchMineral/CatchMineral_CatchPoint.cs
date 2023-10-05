using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildCatch
{
    public class CatchMineral_CatchPoint : CatchBase_CatchPoint
    {
        private SpriteRenderer spriteRenderer;

        protected override void Start()
        {
            base.Start();

            spriteRenderer = transform.Find("MineralSprite").GetComponent<SpriteRenderer>();
            var spriteList = ((CatchMineral_Manager)manager).MineralSprites;
            spriteRenderer.sprite = spriteList[Random.Range(0, spriteList.Count)];
        }
    }
}

