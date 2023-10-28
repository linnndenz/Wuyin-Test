using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaData
{
    public readonly string Name_EN;
    public readonly string Name_CN;
    public int Num;

    public GachaData(string enName, string cnName, int num = 1)
    {
        Name_EN = enName;
        Name_CN = cnName;
        Num = num;
    }
}

public class GachaDataManager : MonoBehaviour
{
    public Dictionary<string, string> GachaNameDict = new() {
        { "alima", "阿丽玛" },
        { "bayue", "八月" },
        { "boboke", "波波克" },
        { "chuangyezhe", "创业者" },
        { "jiya", "吉雅" },
        { "lanhua", "兰花" },
        { "leibo", "雷伯" },
        { "niushu", "牛叔" },
        { "sanmu", "三目" },
        { "sangqingnian", "丧青年" }
    };
    /// <summary>
    /// 扭蛋总数，根据GachaNameDict计算
    /// </summary>
    public int GachaSum { get; private set; }

    #region 存档数据
    private int coin;
    private int twistCount; // 可扭次数
    private List<GachaData> gachaDataList;
    #endregion

    public int Coin { get { return coin; } set { coin = value; } }
    public int TwistCount { get { return twistCount; } set { twistCount = value; } }
    public List<GachaData> GachaDataList { get { return gachaDataList; } }

    public static GachaDataManager Instance {
        get { return s_Instance; }
    }
    private static GachaDataManager s_Instance;

    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one GachaManager script.  The instances are " + s_Instance.name + " and " + name + ".");

        InitData();
    }

    public void InitData()
    {
        // TODO: 数据读取
        coin = 20;
        twistCount = 0;
        if (gachaDataList == null) {
            gachaDataList = new List<GachaData>();
        }

        GachaSum = GachaNameDict.Count;

    }

    #region data
    /// <summary>
    /// 随机获得一个扭蛋
    /// </summary>
    /// <param name="indexInDataList"> 获得的扭蛋在gachaDataList中的序号，若为第一次获得，则为-1 </param>
    public GachaData GainRandomGacha(out int indexInDataList)
    {
        int index = Random.Range(0, GachaSum);
        int cnt = 0;
        KeyValuePair<string, string> newGachaName = default;
        foreach (var n in GachaNameDict) {
            if (cnt == index) {
                newGachaName = n;
                break;
            }
            cnt++;
        }

        var newGacha = gachaDataList.Find((data) => data.Name_EN == newGachaName.Key);
        if (newGacha == null) {
            newGacha = new GachaData(newGachaName.Key, newGachaName.Value);
            gachaDataList.Insert(0, newGacha);

            indexInDataList = -1;
        } else {
            newGacha.Num++;

            indexInDataList = gachaDataList.IndexOf(newGacha);
        }
        return newGacha;
    }

    /// <summary>
    /// 收集率计算
    /// </summary>
    public int GetCollectionRate()
    {
        float cnt = 0;
        foreach (var gachaData in GachaDataList) {
            if (gachaData.Num > 0) cnt++;
        }
        return (int)(cnt / GachaSum * 100);
    }
    #endregion


}
