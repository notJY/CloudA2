using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class StatDisplayManager : MonoBehaviour
{
    [SerializeField] StatDisplay[] statDisplays;
    [SerializeField] Inventory inventoryManager;
    [SerializeField] TMP_Text statPoints;
    private bool isAwake, JSONSent, JSONLoaded;
    public static List<Stat> statList = new List<Stat>();

    private void OnEnable()
    {
        if (!isAwake)
        {
            InitStatList();
            LoadJSON();
            isAwake = true;

            //GameObject scale starts at (0, 0, 0) so it can be hidden and initialize variables at the same time
            transform.localScale = new Vector3(1, 1, 1);

            gameObject.SetActive(false);
        }
        else
        {
            UpdateStatPoints(); 
        }
    }

    public void OnUpgradeButton(int statIndex)
    {
        StartCoroutine(UpgradeStat(statIndex));
    }

    IEnumerator UpgradeStat(int statIndex)
    {
        inventoryManager.OnItemUse("Stat Point");
        yield return new WaitUntil(()=> inventoryManager.usedItemName != "");

        if (inventoryManager.usedItemName == "Stat Point")
        {
            //Increase stat value
            float statValue;
            statValue = float.Parse(statList[statIndex].value);
            statValue += statList[statIndex].upgradeMultiplier;
            statList[statIndex].value = statValue.ToString();

            //Reload JSONs
            JSONSent = false;
            JSONLoaded = false;
            SendJSON();
            yield return new WaitUntil(()=> JSONSent);
            LoadJSON();
            yield return new WaitUntil(()=> JSONLoaded);
            UpdateStatPoints();
            Debug.Log("Stat point used");
        }
        inventoryManager.usedItemName = "";
    }

    void UpdateStatPoints()
    {
        if ((inventoryManager.inventory == null) || (inventoryManager.inventory.Count == 0))
        {
            statPoints.text = "Stat Points: 0";
            return;
        }

        foreach (ItemInstance item in inventoryManager.inventory)
        {
            if (item.DisplayName == "Stat Point")
            {
                statPoints.text = "Stat Points: " + item.RemainingUses;
                break;
            }
            else if (item == inventoryManager.inventory[inventoryManager.inventory.Count - 1])
            {
                statPoints.text = "Stat Points: 0";
                break;
            }
        }
    }

    void InitStatList()
    {
        if (statList.Count < statDisplays.Length)
        {
            foreach (var item in statDisplays)
            {
                statList.Add(item.ReturnClass());
            }
        }
        else
        {
            for (int i = 0; i < statDisplays.Length; i++)
            {
                statList[i] = statDisplays[i].ReturnClass();
            }
        }
    }

    void SendJSON()
    {
        string stringListAsJSON = JsonUtility.ToJson(new JSONListWrapper<Stat>(statList));
        Debug.Log("JSON data prepared: " + stringListAsJSON);
        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Stats", stringListAsJSON }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, OnJSONDataSent, OnError);
    }

    void OnJSONDataSent(UpdateUserDataResult r)
    {
        JSONSent = true;
        Debug.Log("Data sent success!");
    }

    void LoadJSON()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, OnError);
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        Debug.Log("Received JSON data");
        if (r.Data != null && r.Data.ContainsKey("Stats"))
        {
            Debug.Log(r.Data["Stats"].Value);
            JSONListWrapper<Stat> jlw = JsonUtility.FromJson<JSONListWrapper<Stat>>(r.Data["Stats"].Value);
            for (int i = 0; i < statDisplays.Length; i++)
            {
                statDisplays[i].SetUI(jlw.list[i]);
            }

            JSONLoaded = true;
            InitStatList();
        }
    }

    void OnError(PlayFabError e)
    {
        Debug.Log("Error: " + e.GenerateErrorReport());
    }
}

[System.Serializable]
public class JSONListWrapper<T>
{
    public List<T> list;
    public JSONListWrapper(List<T> list) => this.list = list;
}