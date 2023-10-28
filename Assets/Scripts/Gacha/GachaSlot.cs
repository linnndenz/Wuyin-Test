using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaSlot : MonoBehaviour
{
    private GameObject details;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI numText;
    private Image gachaPicImage;

    private Sprite gachaPic;

    void Awake()
    {
        details = transform.GetChild(0).gameObject;
        gachaPicImage = transform.Find("GachaPic").GetComponent<Image>();
        nameText = details.transform.Find("Text_Name").GetComponent<TextMeshProUGUI>();
        numText = details.transform.Find("Text_Num").GetComponent<TextMeshProUGUI>();

        blankGachaPic = gachaPicImage.sprite;
    }

    public void InitSlot(string name, int num, Sprite pic)
    {
        nameText.text = name;
        gachaPic = pic;

        if (num > 0) {
            numText.text = num.ToString();
            details.SetActive(true);
            gachaPicImage.sprite = gachaPic;
        } else {
            details.SetActive(false);
        }
    }

    public void SetSlotNum(int num)
    {
        numText.text = num.ToString();
    }

    Sprite blankGachaPic;
    public void ResetSlot()
    {
        details.SetActive(false);
        gachaPicImage.sprite = blankGachaPic;
    }

}
