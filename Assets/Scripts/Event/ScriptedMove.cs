using UnityEngine.SceneManagement;
using UnityEngine;

public class ScriptedMove : MonoBehaviour
{
    public bool SwtichScene = false;
    public string SceneName;
    public GameObject MovePoints;
    public GameObject[] CoverPlayerObjs;
    public GameObject[] PlayerCoverObjs;

    public void StartMove()
    {
        AdjustObjLayer(false);
        var pointsNum = MovePoints.transform.childCount;
        Vector2[] targets = new Vector2[pointsNum];
        for (int i = 0; i < pointsNum; ++i)
        {
            targets[i] = MovePoints.transform.GetChild(i).position;
        }

        GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        PlayerCharacter.Instance.SetTargetPoints(targets);
        PlayerCharacter.Instance.SetMoveFinishAction(FinishMove);
        PlayerCharacter.Instance.PassInputSignal(2);
    }

    public void StartMove(GameObject points, bool reset = false, bool switchScene = false)
    {
        SwtichScene = switchScene;

        AdjustObjLayer(reset);
        var pointsNum = points.transform.childCount;
        Vector2[] targets = new Vector2[pointsNum];
        for (int i = 0; i < pointsNum; ++i)
        {
            targets[i] = points.transform.GetChild(i).position;
        }

        GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        PlayerCharacter.Instance.SetTargetPoints(targets);
        PlayerCharacter.Instance.SetMoveFinishAction(FinishMove);
        PlayerCharacter.Instance.PassInputSignal(2);
    }

    public void FinishMove()
    {
        if (SwtichScene)
        {
            if (SceneName.Length > 0)
            {
                SceneManager.LoadScene(SceneName);
            }
        }
    }

    public void AdjustObjLayer(bool reset)
    {
        string coverLayer = "CoverChar";
        string uncoverLayer = "CoveredByChar";
        // revert layer changes
        if (reset == true)
        {
            coverLayer = "CoveredByChar";
            uncoverLayer = "CoverChar";
        }

        SpriteRenderer sprite;
        for (int i = 0; i < CoverPlayerObjs.Length; ++i)
        {
            sprite = CoverPlayerObjs[i].GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = coverLayer;
        }
        for (int i = 0; i < PlayerCoverObjs.Length; ++i)
        {
            sprite = PlayerCoverObjs[i].GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = uncoverLayer;
        }
    }
}