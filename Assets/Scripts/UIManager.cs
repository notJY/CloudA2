using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] TMP_Text highscoreTxt, currencyTxt;
    [HideInInspector] public int highscore, currency;

    [SerializeField] TMP_Text inGameMsg;
    public bool isMenu;

    private void Awake()
    {
        GetHighscore();
        if (isMenu)
        {
            GetCurrency();
        }
    }

    public void OnBackButton(GameObject currentUI)
    {
        currentUI.SetActive(false);
        mainMenu.SetActive(true);
        GetHighscore();
    }

    public void OnUIButton(GameObject newUI)
    {
        mainMenu.SetActive(false);
        newUI.SetActive(true);
    }

    void OnError(PlayFabError e)
    {
        highscoreTxt.text = "Error: " + e.GenerateErrorReport();
    }

    void GetHighscore()
    {
        var statsReq = new GetPlayerStatisticsRequest()
        {
            StatisticNames = new List<string> ()
            {
                "Highscore"
            }
        };
        PlayFabClientAPI.GetPlayerStatistics(statsReq, OnHighscoreReceived, OnError);
    }

    void OnHighscoreReceived(GetPlayerStatisticsResult r)
    {
        if (!isMenu && (r.Statistics != null) && (r.Statistics.Count > 0))
        {
            highscore = r.Statistics[0].Value;
        }
        else if (r.Statistics != null)
        {
            highscoreTxt.text = "Highscore: \n" + r.Statistics[0].Value;
            highscore = r.Statistics[0].Value;
        }
        else
        {
            Debug.Log(r.Statistics);
            highscoreTxt.text = "Highscore: 0";
            highscore = 0;
        }
    }

    public void GetCurrency()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInventoryReceived, OnError);
    }

    void OnInventoryReceived(GetUserInventoryResult r)
    {
        currency = r.VirtualCurrency["GD"];
        currencyTxt.text = currency.ToString();
    }

    public void AddCurrency(int amount)
    {
        var addCurrencyReq = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "GD",
            Amount = amount
        };
        PlayFabClientAPI.AddUserVirtualCurrency(addCurrencyReq, OnCurrencyModified, OnCurrencyError);
    }

    void OnCurrencyError(PlayFabError e)
    {
        inGameMsg.text = "Error: " + e.GenerateErrorReport();
    }

    void OnCurrencyModified(ModifyUserVirtualCurrencyResult r)
    {
        currency = r.Balance;
        inGameMsg.text = "Gold earned: " + r.BalanceChange.ToString();
    }
}
