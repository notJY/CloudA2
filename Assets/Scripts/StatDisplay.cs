using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Stat
{
    public string name, value; //TMP text cannot parse int so have to store value as string
    public float upgradeMultiplier;
    public Stat(string _name, string _value, float _multiplier)
    {
        name = _name;
        value = _value;
        upgradeMultiplier = _multiplier;
    }
}

public class StatDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text statName, statValue;
    [SerializeField] Button upgradeButton;
    public float upgradeMultiplier;

    public Stat ReturnClass()
    {
        return new Stat(statName.text, statValue.text, upgradeMultiplier);
    }

    public void SetUI(Stat stat)
    {
        statName.text = stat.name;
        statValue.text = stat.value;
    }
}
