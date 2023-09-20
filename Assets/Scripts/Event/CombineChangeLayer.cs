using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineChangeLayer : MonoBehaviour
{
    public GameObject[] objectList;

    private SpriteRenderer m_Sprite;

    public void UpdateCombinedObjecsLayer()
    {
        m_Sprite = GetComponent<SpriteRenderer>();
        for (int i = 0; i < objectList.Length; ++i)
        {
            SpriteRenderer sprite = objectList[i].GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = m_Sprite.sortingLayerName;
        }
    }
}