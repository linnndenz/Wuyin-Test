using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public float WalkingSpeed = 6;
    public int Health;
    public int Attack;
    public int Defense;
    [HideInInspector] public int Day;

    public void Init(float speed, int health, int attack, int defense, int day)
    {
        WalkingSpeed = speed;
        Health = health;
        Attack = attack;
        Defense = defense;
        Day = day;
    }

    public override string ToString()
    {
        string s = "";

        s += "\"Player\": {";
        s += "\"Day\": " + Day.ToString() + ", ";
        s += "\"WalkingSpeed\": " + WalkingSpeed.ToString() + ", ";
        s += "\"Health\": " + Health.ToString() + ", ";
        s += "\"Attack\": " + Attack.ToString() + ", ";
        s += "\"Defense\": " + Defense.ToString() + ", ";
        s += "\"DateTime\": " + "\"" +DateTime.Now.ToString("yyyy+MM-dd*") + "  " + DateTime.Now.ToString("HH : mm")+ "\""+", ";        

        Vector2 pos = GameObject.FindGameObjectWithTag("Player").transform.position;
        s += "\"posX\": " + pos.x.ToString() + ", ";
        s += "\"posY\": " + pos.y.ToString();

        s += "}";


        return s;
    }
}