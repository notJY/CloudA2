using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PFUserLogout : MonoBehaviour
{
    public void OnLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        SceneManager.LoadScene("SampleScene");
    }
}
