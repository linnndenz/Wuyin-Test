using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildCatch
{
    public class CatchPlant_CatchPoint : CatchBase_CatchPoint
    {
        public enum EPlantStyle { PLANT, MUSHROOM }
        [Header("植物种类")]
        public EPlantStyle PlantStyle;

        private SpriteRenderer spriteRenderer;

        protected override void Start()
        {
            base.Start();

            spriteRenderer = transform.Find("PlantSprite").GetComponent<SpriteRenderer>();
            var spriteList = new List<Sprite>();
            switch (PlantStyle) {
                case EPlantStyle.PLANT:
                    spriteList = ((CatchPlant_Manager)manager).PlantSprites;
                    break;
                case EPlantStyle.MUSHROOM:
                    spriteList = ((CatchPlant_Manager)manager).MushroomSprites;
                    break;
            }
            spriteRenderer.sprite = spriteList[Random.Range(0, spriteList.Count)];
        }
    }
}