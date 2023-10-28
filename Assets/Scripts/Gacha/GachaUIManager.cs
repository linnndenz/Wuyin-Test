using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaUIManager : MonoBehaviour
{
    [Header("sprite")]
    [SerializeField] private List<Sprite> gachaPics;
    [SerializeField] private Sprite[] ballPics;
    [SerializeField] private Sprite[] knob_grey_sprite;

    [Header("body")]
    [SerializeField] private Animator insertCoinAnim;

    [SerializeField] private Button onceBtn;
    [SerializeField] private Button quinticBtn;

    [SerializeField] private Transform knob_once;
    [SerializeField] private Transform knob_quintic;

    [SerializeField] private Transform ballContainer;
    Transform[] balls;

    [SerializeField] private CanvasGroup ballCanvasGroup;
    [SerializeField] private CanvasGroup gainMaskCanvasGroup;
    private Image gainGachaPic;
    private GameObject gainNewTip;

    [Header("data visual")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI twistCountText;
    [SerializeField] private TextMeshProUGUI collectionRateText;

    [Header("grid")]
    [SerializeField] private Transform gachaSlotContainer;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Slider slider;

    [Header("slider按钮单次移动长度")]
    [SerializeField] private float sliderBtnStep = 0.2f;

    GachaDataManager dataManager;
    bool isTwisting;
    GachaSlot[] gachaSlotList;
    void Start()
    {
        dataManager = GachaDataManager.Instance;
        gachaSlotList = gachaSlotContainer.GetComponentsInChildren<GachaSlot>();
        UpdateGachaSlot();

        // 超过12个格子显示滚动条
        if (gachaSlotContainer.childCount > 12)
            slider.transform.parent.gameObject.SetActive(true);
        else
            slider.transform.parent.gameObject.SetActive(false);

        insertCoinAnim.GetComponent<CanvasGroup>().alpha = 0;
        ballCanvasGroup.alpha = 0;

        gainMaskCanvasGroup.gameObject.SetActive(false);
        gainGachaPic = gainMaskCanvasGroup.transform.GetChild(0).GetComponent<Image>();
        gainNewTip = gainGachaPic.transform.GetChild(0).gameObject;

        knob_normal_sprite = new Sprite[2];
        knob_normal_sprite[0] = knob_once.GetComponent<Image>().sprite;
        knob_normal_sprite[1] = knob_quintic.GetComponent<Image>().sprite;

        balls = new Transform[ballContainer.childCount];
        for (int i = 0; i < ballContainer.childCount; i++) {
            balls[i] = ballContainer.GetChild(i);
        }

        SetGachaBtnInteractable(dataManager.TwistCount);

        // 数据显示
        coinText.text = dataManager.Coin.ToString();
        twistCountText.text = dataManager.TwistCount.ToString();
        collectionRateText.text = dataManager.GetCollectionRate().ToString() + "%";

    }

    void Update()
    {
        // slider样式替换scrollbar
        if (slider.isActiveAndEnabled) {
            scrollbar.value = slider.value;
        }
    }

    /// <summary>
    /// TODO: 点击添加扭蛋币按钮
    /// </summary>
    public void OnAddCoinBtnClicked()
    {

    }

    /// <summary>
    /// 点击grid滚动条按钮
    /// </summary>
    public void OnSliderBtnClicked(int dir)
    {
        slider.value += dir * sliderBtnStep;
    }

    /// <summary>
    /// 点击投入硬币
    /// </summary>
    public void OnInsertCoinBtnClicked()
    {
        // 上一次投币未结束
        if (insertCoinAnim.GetComponent<CanvasGroup>().alpha > 0.1f) return;

        // 数据更新
        if (dataManager.Coin <= 0) return; // 硬币不足
        dataManager.Coin--;
        dataManager.TwistCount++;

        // UI显示
        insertCoinAnim.GetComponent<CanvasGroup>().alpha = 1;
        insertCoinAnim.Play("InsertCoin");

        coinText.text = dataManager.Coin.ToString();
        twistCountText.text = dataManager.TwistCount.ToString();

        SetGachaBtnInteractable(dataManager.TwistCount);
    }

    Sprite[] knob_normal_sprite;
    /// <summary>
    /// 设置扭1次/扭5次按钮是否可交互
    /// </summary>
    private void SetGachaBtnInteractable(int twistCnt)
    {
        if (twistCnt > 0) {
            knob_once.GetComponent<Image>().sprite = knob_normal_sprite[0];
            onceBtn.interactable = true;
        } else {
            onceBtn.interactable = false;
            knob_once.GetComponent<Image>().sprite = knob_grey_sprite[0];
        }

        if (twistCnt >= 5) {
            knob_quintic.GetComponent<Image>().sprite = knob_normal_sprite[1];
            quinticBtn.interactable = true;
        } else {
            quinticBtn.interactable = false;
            knob_quintic.GetComponent<Image>().sprite = knob_grey_sprite[1];
        }


    }

    /// <summary>
    /// 点击扭1次按钮
    /// </summary>
    public void OnTwistOnceBtnClicked()
    {
        if (dataManager.TwistCount <= 0) throw new UnityException("扭蛋次数异常");
        if (isTwisting) return;

        // 数据更新
        dataManager.TwistCount--;
        isTwisting = true;

        // UI显示
        twistCountText.text = dataManager.TwistCount.ToString();
        SetGachaBtnInteractable(dataManager.TwistCount);

        StartCoroutine(OnTwistBtnClicked(knob_once, 1));
    }

    /// <summary>
    /// 点击扭5次按钮
    /// </summary>
    public void OnTwistQuinticBtnClicked()
    {
        if (dataManager.TwistCount < 5) throw new UnityException("扭蛋次数异常");
        if (isTwisting) return;

        // 数据更新
        dataManager.TwistCount -= 5;
        isTwisting = true;

        // UI显示
        twistCountText.text = dataManager.TwistCount.ToString();
        SetGachaBtnInteractable(dataManager.TwistCount);

        StartCoroutine(OnTwistBtnClicked(knob_quintic, 5));
    }

    IEnumerator OnTwistBtnClicked(Transform knob, int twistCnt)
    {
        knob.DOKill();
        knob.localEulerAngles = Vector3.zero;
        knob.DOLocalRotate(-90 * Vector3.forward, 0.5f);
        ShakeBalls();
        yield return new WaitForSeconds(1f);
        knob.DOLocalRotate(-180 * Vector3.forward, 0.5f);
        ShakeBalls();
        yield return new WaitForSeconds(0.5f);

        ballCanvasGroup.GetComponent<Image>().sprite = ballPics[Random.Range(0, ballPics.Length)];
        ballCanvasGroup.DOFade(1, 1f);
        yield return new WaitForSeconds(1);

        gainMaskCanvasGroup.alpha = 1;
        gainMaskCanvasGroup.gameObject.SetActive(true);
        gainMaskCanvasGroup.DOFade(1, 1f);

        for (int i = 0; i < twistCnt; i++) {
            GainGacha();
            yield return new WaitForSeconds(1.5f);
        }

        gainMaskCanvasGroup.DOFade(0, 1f).OnComplete(() => {
            gainMaskCanvasGroup.gameObject.SetActive(false);
        });
        ballCanvasGroup.DOFade(0, 1f);

        isTwisting = false;
    }
    /// <summary>
    /// 获得扭蛋，出获得扭蛋的提示mask
    /// </summary>
    private void GainGacha()
    {
        var gacha = dataManager.GainRandomGacha(out int index);

        // UI设置
        gainGachaPic.sprite = gachaPics.Find((pic) => pic.name.Replace("gacha_", string.Empty) == gacha.Name_EN);
        gainGachaPic.transform.localScale = Vector3.zero;
        gainGachaPic.transform.DOScale(Vector3.one, 0.5f);

        if (index == -1) {
            gainNewTip.gameObject.SetActive(true);
            UpdateGachaSlot();
        } else {
            gainNewTip.gameObject.SetActive(false);
            gachaSlotList[index].SetSlotNum(gacha.Num);
        }

        collectionRateText.text = dataManager.GetCollectionRate().ToString() + "%";

    }

    /// <summary>
    /// 重新刷一遍 gacha slot
    /// </summary>
    private void UpdateGachaSlot()
    {
        for (int i = 0; i < gachaSlotList.Length; i++) {
            if (i < dataManager.GachaDataList.Count) {
                var gachaData = dataManager.GachaDataList[i];
                gachaSlotList[i].InitSlot(gachaData.Name_CN, gachaData.Num, gachaPics.Find(
                    (pic) => pic.name.Replace("gacha_", string.Empty) == gachaData.Name_EN));
            } else {
                gachaSlotList[i].ResetSlot();
            }
        }
    }

    /// <summary>
    /// 旋转旋钮时玻璃窗内球摇晃
    /// </summary>
    private void ShakeBalls()
    {
        foreach (var ball in balls) {
            ball.DOKill();
            ball.DORotate(new Vector3(0, 0, Random.Range(0, 360)), 0.7f);
            ball.DOShakePosition(0.7f, Random.Range(30, 100), Random.Range(5, 10), 20);
        }
    }
}
