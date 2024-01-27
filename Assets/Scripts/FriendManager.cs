using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Trade
{
    public string senderId, itemName, tradeId;

    public Trade(string _senderId, string _itemName, string _tradeId)
    {
        senderId = _senderId;
        itemName = _itemName;
        tradeId = _tradeId;
    }
}

public class FriendManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtFriendAdded, txtUnfriended, txtSelectedItem, txtSent, txtAccepted;
    [SerializeField] TMP_InputField tgtFriend, txtFrdList, playerIDInput, tradeIDInput, txtPendingGifts;
    [SerializeField] GameObject mainMenu, friendUI, tradeUI;
    [SerializeField] Inventory inventoryManager;
    [SerializeField] Shop shop;

    private List<FriendInfo> _friends = null;
    private ItemInstance selectedItem = null;
    private List<Trade> tradesList = new List<Trade>();
    private List<Image> inventorySlots = new List<Image>();

    private void OnEnable()
    {
        //Get inventory image gameObjects in children
        Image[] invImages = tradeUI.GetComponentsInChildren<Image>();
        foreach (Image img in invImages)
        {
            //If image belongs to an inventory slot, add to list
            if (img.gameObject.tag == "Inventory Slot")
            {
                inventorySlots.Add(img);
            }
        }
    }

    public void OnFriendButton()
    {
        mainMenu.SetActive(false);
        friendUI.SetActive(true);
        txtFriendAdded.enabled = false;
        txtUnfriended.enabled = false;

        if (_friends == null)
        {
            GetFriends();
        }
    }

    void AddFriend(string friendId)
    {
        var request = new AddFriendRequest()
        {
            FriendPlayFabId = friendId
        };
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
            txtFriendAdded.enabled = true;
            _friends = null;
        }, DisplayPlayFabError);
    }
    public void OnAddFriend()
    { 
        if (tgtFriend.text == "")
        {
            return;
        }

        //to add friend based on ID
        AddFriend(tgtFriend.text);
    }

    public void OnUnFriend()
    {
        if (tgtFriend.text == "")
        {
            return;
        }

        RemoveFriend(tgtFriend.text);
    }
    void RemoveFriend(string pfid)
    {
        var req = new RemoveFriendRequest
        {
            FriendPlayFabId = pfid
        };
        PlayFabClientAPI.RemoveFriend(req
        , result => {
            Debug.Log("unfriended!");
            txtUnfriended.enabled = true;
            _friends = null;
        }, DisplayPlayFabError);
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            _friends = result.Friends;
            if (_friends.Count > 0)
            {
                DisplayFriends(_friends); // triggers your UI
            }
            else
            {
                txtFrdList.text = "";
            }
        }, DisplayPlayFabError);
    }

    void DisplayFriends(List<FriendInfo> friendsCache)
    {
        txtFrdList.text = "";
        friendsCache.ForEach(f => {
            Debug.Log(f.FriendPlayFabId + "," + f.TitleDisplayName);
            txtFrdList.text += f.TitleDisplayName + "[" + f.FriendPlayFabId + "]\n";
            if (f.Profile != null) Debug.Log(f.FriendPlayFabId + "/" + f.Profile.DisplayName);
        });
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    //Separate function from inventoryManager for separate inventory
    void DisplayInventory()
    {
        if ((inventoryManager.inventory == null) || (inventoryManager.inventory.Count == 0))
        {
            //If inventory is empty, set all slot images to null
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].sprite != null)
                {
                    inventorySlots[i].sprite = null;
                    inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g,
                                                        inventorySlots[i].color.b, 0f);
                    inventorySlots[i].GetComponentsInChildren<Image>()[1].enabled = true;
                }
            }
            return;
        }

        //Loop through player inventory
        for (int i = 0; i < inventoryManager.inventory.Count; i++)
        {
            //Loop through each item ID
            for (int k = 0; k < shop.shopItems.Count; k++)
            {
                //If ID matches the inventory item ID, set the image in the slot
                if (inventoryManager.inventory[i].ItemId == shop.shopItems[k].ItemId)
                {
                    inventorySlots[i].sprite = shop.shopItemImages[k];
                    inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g,
                                                        inventorySlots[i].color.b, 1f);
                    inventorySlots[i].GetComponentsInChildren<Image>()[1].enabled = false;

                    //Show item count if more than 1 use
                    if (inventoryManager.inventory[i].RemainingUses > 1)
                    {
                        TMP_Text itemCount = inventorySlots[i].GetComponentInChildren<TMP_Text>(includeInactive: true);
                        itemCount.gameObject.SetActive(true);
                        itemCount.text = inventoryManager.inventory[i].RemainingUses.ToString();
                    }
                }
                else if (i == inventoryManager.inventory.Count)
                {
                    //If the last item has been set, loop through the rest of the slots and if sprite !null, set to null
                    for (int e = i; e < inventorySlots.Count; e++)
                    {
                        if (inventorySlots[i].sprite != null)
                        {
                            inventorySlots[i].sprite = null;
                            inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g,
                                                                inventorySlots[i].color.b, 0f);
                            inventorySlots[i].GetComponentsInChildren<Image>()[1].enabled = true;
                        }
                    }
                }
            }
        }
    }

    public void OnSelectItem(Image currentSlotImage)
    {
        //Get item name from shop by comparing images
        for (int i = 0; i < shop.shopItems.Count; i++)
        {
            if (currentSlotImage.sprite == shop.shopItemImages[i])
            {
                //Get item instance from inventory using item name
                foreach (ItemInstance item in inventoryManager.inventory)
                {
                    if ((item.DisplayName == shop.shopItems[i].DisplayName) && (item.DisplayName != "Stat Point")) //Stat points cannot be traded
                    {
                        txtSelectedItem.text = "Selected Item:\n" + item.DisplayName;
                        selectedItem = item;
                        break;
                    }
                }
                break;
            }
        }
    }

    public void SendItem()
    {
        if ((selectedItem == null) || (playerIDInput.text == ""))
        {
            return;
        }

        GiveItemTo(playerIDInput.text, selectedItem.ItemInstanceId);
    }

    void GiveItemTo(string secondPlayerID, string myItemInstanceID)
    {
        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { secondPlayerID },
            OfferedInventoryInstanceIds = new List<string> { myItemInstanceID },
        }, OpenTradeSuccess, DisplayPlayFabError);
    }

    void OpenTradeSuccess(OpenTradeResponse r)
    {
        tradesList.Add(new Trade(r.Trade.OfferingPlayerId, selectedItem.DisplayName, r.Trade.TradeId));
        Debug.Log(tradesList[0] + "   " + r.Trade.TradeId);
        SendJSON();
    }

    //Send pending trades to the target player as player title data through the server
    void SendJSON()
    {
        string stringListAsJSON = JsonUtility.ToJson(new JSONListWrapper<Trade>(tradesList));
        Debug.Log("JSON data prepared: " + stringListAsJSON);
        var sendTradesList = new ExecuteCloudScriptRequest
        {
            FunctionName = "UpdateUserData",
            FunctionParameter = new
            {
                playerId = playerIDInput.text,
                data = new Dictionary<string, string>
                       {
                           {"Pending Trades", stringListAsJSON }
                       }
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(sendTradesList, OnJSONDataSent, DisplayPlayFabError);
    }

    void OnJSONDataSent(ExecuteCloudScriptResult r)
    {
        txtSent.enabled = true;
        inventoryManager.GetInventory();
    }

    void LoadJSON()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, DisplayPlayFabError);
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        Debug.Log("Received JSON data");
        if (r.Data != null && r.Data.ContainsKey("Pending Trades"))
        {
            Debug.Log(r.Data["Pending Trades"].Value);
            tradesList = JsonUtility.FromJson<JSONListWrapper<Trade>>(r.Data["Pending Trades"].Value).list;
            Debug.Log(tradesList.Count);
            txtPendingGifts.text = "";
            if ((tradesList != null) && (tradesList.Count > 0))
            {
                tradesList.ForEach(f =>
                {
                    txtPendingGifts.text += "Sender ID: " + f.senderId + "\nItem: " + f.itemName + "\nTrade ID: " + f.tradeId + "\n";
                });
            }
        }
    }

    //Unused
    void ExamineTrade(string firstPlayfabID, string tradeID)
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = firstPlayfabID,
            TradeId = tradeID
        }, TradeStatusSuccess, DisplayPlayFabError);
    }

    void TradeStatusSuccess(GetTradeStatusResponse r)
    {
        Debug.Log("Success!");
    }
    //

    public void OnAcceptGiftButton()
    {
        if ((playerIDInput.text == "") || (tradeIDInput.text == ""))
        {
            return;
        }

        AcceptGiftFrom(playerIDInput.text, tradeIDInput.text);
    }

    void AcceptGiftFrom(string firstPlayfabID, string tradeID)
    {
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            OfferingPlayerId = firstPlayfabID,
            TradeId = tradeID
        }, AcceptTradeSuccess, DisplayPlayFabError);
    }

    void AcceptTradeSuccess(AcceptTradeResponse r)
    {
        txtAccepted.enabled = true;
        inventoryManager.GetInventory();
    }

    public void OnTradeButton()
    {
        LoadJSON();
        friendUI.SetActive(false);
        tradeUI.SetActive(true);
        txtSent.enabled = false;
        txtAccepted.enabled = false;

        DisplayInventory();
    }

    public void OnTradeBackButton()
    {
        tradeUI.SetActive(false);
        friendUI.SetActive(true);
    }
}
