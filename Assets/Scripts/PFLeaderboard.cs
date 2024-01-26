using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFLeaderboard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI leaderboardtxt;
    [SerializeField] GameObject mainMenu, leaderboard;

    void OnError(PlayFabError e)
    {
        leaderboardtxt.text = "Error: " + e.GenerateErrorReport();
    }

    public void OnLeaderboardButton()
    {
        mainMenu.SetActive(false);
        leaderboard.SetActive(true);
        OnButtonGetGlobalLeaderboard();
    }

    public void OnButtonGetGlobalLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "Highscore",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnGlobalLeaderboardGet, OnError);
    }

    void OnGlobalLeaderboardGet(GetLeaderboardResult r)
    {
        string leaderboardStr = "Global Leaderboard\n";
        foreach (var item in r.Leaderboard)
        {
            if ((item.DisplayName == null) || (item.DisplayName == ""))
            {
                string oneRow = (item.Position + 1) + " Guest " + item.StatValue + "\n";
                leaderboardStr += oneRow;
            }
            else
            {
                string oneRow = (item.Position + 1) + " " + item.DisplayName + " " + item.StatValue + "\n";
                leaderboardStr += oneRow;
            }
        }
        leaderboardtxt.text = leaderboardStr;
    }

    public void OnButtonGetNearbyLeaderboard()
    {
        var lbreq = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Highscore",
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(lbreq, OnNearbyLeaderboardGet, OnError);
    }

    void OnNearbyLeaderboardGet(GetLeaderboardAroundPlayerResult r)
    {
        string leaderboardStr = "Nearby Leaderboard\n";
        foreach (var item in r.Leaderboard)
        {
            if ((item.DisplayName == null) || (item.DisplayName == ""))
            {
                string oneRow = (item.Position + 1) + " Guest " + item.StatValue + "\n";
                leaderboardStr += oneRow;
            }
            else
            {
                string oneRow = (item.Position + 1) + " " + item.DisplayName + " " + item.StatValue + "\n";
                leaderboardStr += oneRow;
            }
        }
        leaderboardtxt.text = leaderboardStr;
    }

    public void OnSendLeaderboard(int score)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate
                {
                    StatisticName = "Highscore",
                    Value = score
                }
            }
        };
        //leaderboardtxt.text = "Submitting score: " + score;
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        Debug.Log("Successful leaderboard send: " + r.ToString());
    }

    public void OnButtonGetFriendLeaderboard()
    {
        PlayFabClientAPI.GetFriendLeaderboard(
        new GetFriendLeaderboardRequest { StatisticName = "Highscore", MaxResultsCount = 10 },
        r =>
        {
            string leaderboardStr = "Friends Leaderboard\n";
            foreach (var item in r.Leaderboard)
            {
                if ((item.DisplayName == null) || (item.DisplayName == ""))
                {
                    string oneRow = (item.Position + 1) + " Guest " + item.StatValue + "\n";
                    leaderboardStr += oneRow;
                }
                else
                {
                    string oneRow = (item.Position + 1) + " " + item.DisplayName + " " + item.StatValue + "\n";
                    leaderboardStr += oneRow;
                }
            }
            leaderboardtxt.text = leaderboardStr;
        }, OnError);
    }
}