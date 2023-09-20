using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;

public class ObjectInteraction : MonoBehaviour
{
    public string Type;
    public GameObject MovePoints;
    public GameObject Image;
    public GameObject[] IgnoreObjectList;
    public string ForceAnimation;

    public void StartMove()
    {
        if (ForceAnimation != null && ForceAnimation.Length > 0)
        {
            PlayerCharacter.Instance.SetForceAnimation(ForceAnimation);
        }

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
        PlayerCharacter.Instance.SetScriptMoving(true);
    }

    public void FinishMove()
    {
        GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        if (ForceAnimation != null && ForceAnimation.Length > 0)
        {
            PlayerCharacter.Instance.SetForceAnimation("");
        }
        if (Image != null)
        {
            Image.SetActive(false);
        }
        if (IgnoreObjectList != null)
        {
            foreach (var trigger in IgnoreObjectList)
            {
                trigger.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayerCharacter.Instance.IsScriptMoving() == true)
            return; // Duplicated call

        if (other.transform.gameObject.tag == "Player")
        {
            if (Image != null)
                Image.SetActive(true);
            if (IgnoreObjectList != null)
                foreach (var trigger in IgnoreObjectList)
                {
                    trigger.SetActive(false);
                }

            if (Type.Equals("move"))
            {
                StartMove();
            }
            else if (Type.StartsWith("climb"))
            {

                if (Type.EndsWith("up"))
                {
                    PlayerCharacter.Instance.SetAnimClimbing("up");
                }
                else if (Type.EndsWith("down"))
                {
                    PlayerCharacter.Instance.SetAnimClimbing("down");
                }

                StartMove();
            }
        }
    }

    void OnMouseOver()
    {
        PlayerCharacter.Instance.SetActiveInteractObject(gameObject);
        Debug.Log("youle");
    }

    void OnMouseExit()
    {
        PlayerCharacter.Instance.SetActiveInteractObject(null);
        Debug.Log("meile");
    }
}