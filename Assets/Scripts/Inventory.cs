using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] TMP_Text itemName, itemDesc;
    [SerializeField] Shop shop;
    private bool isAwake, isInit;
    [HideInInspector]public bool isUpdated;
    private List<Image> inventorySlots = new List<Image>();
    [HideInInspector]public List<ItemInstance> inventory = null;
    [HideInInspector]public string usedItemName = "";
    private string itemInstanceID = "";

    //Apparently Awake doesn't work properly with OnEnable, so OnEnable only
    private void OnEnable()
    {
        if (!isAwake)
        {
            isUpdated = false;
            //Get inventory image gameObjects in children
            Image[] invImages = gameObject.GetComponentsInChildren<Image>(); 
            foreach (Image img in invImages)
            {
                //If image belongs to an inventory slot, add to list
                if (img.gameObject.tag == "Inventory Slot")
                {
                    inventorySlots.Add(img);
                }
            }

            isAwake = true;
        }

        if (!isUpdated)
        {
            StartCoroutine(InitInventory());
        }
        else
        {
            ShowInventory();
        }
    }

    IEnumerator InitInventory()
    {
        GetInventory();
        Debug.Log("Updating inv");
        yield return new WaitUntil(() => inventory != null);
        Debug.Log("Show Inv: " + inventory.Count);
        ShowInventory();
        isUpdated = true;

        Debug.Log("Init: " + isInit);
        if (!isInit)
        {
            Debug.Log("Init inv");
            //GameObject scale starts at (0, 0, 0) so it can be hidden and initialize variables at the same time
            transform.localScale = new Vector3(1, 1, 1);

            gameObject.SetActive(false);
            isInit = true;
        }
    }

    public void GetInventory()
    {
        inventory = null;
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInventoryReceived, OnError);
    }

    void OnInventoryReceived(GetUserInventoryResult r)
    {
        inventory = new List<ItemInstance>(r.Inventory);
    }

    void OnError(PlayFabError e)
    {
        Debug.Log("Error: " + e.GenerateErrorReport());
    }

    void ShowInventory()
    {
        if ((inventory == null) || (inventory.Count == 0))
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
        for (int i = 0; i < inventory.Count; i++)
        {
            //Loop through each item ID
            for (int k = 0; k < shop.shopItems.Count; k++)
            {
                //If ID matches the inventory item ID, set the image in the slot
                if (inventory[i].ItemId == shop.shopItems[k].ItemId)
                {
                    inventorySlots[i].sprite = shop.shopItemImages[k];
                    inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g,
                                                        inventorySlots[i].color.b, 1f);
                    inventorySlots[i].GetComponentsInChildren<Image>()[1].enabled = false;

                    //Show item count if more than 1 use
                    if (inventory[i].RemainingUses > 1)
                    {
                        TMP_Text itemCount = inventorySlots[i].GetComponentInChildren<TMP_Text>(includeInactive: true);
                        itemCount.gameObject.SetActive(true);
                        itemCount.text = inventory[i].RemainingUses.ToString();
                    }
                }
                else if (i == inventory.Count)
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

    public void OnItemClick(Image currentSlotImage)
    {
        //Loop through each item image
        for (int k = 0; k < shop.shopItems.Count; k++)
        {
            //Check for same item image
            if (currentSlotImage.sprite == shop.shopItemImages[k])
            {
                itemName.text = shop.shopItems[k].DisplayName;
                itemDesc.text = shop.shopItems[k].Description;
                break;
            }
        }
    }

    public void OnItemUse(string itemName)
    {
        foreach (ItemInstance item in inventory)
        {
            if (item.DisplayName == itemName)
            {
                itemInstanceID = item.ItemInstanceId;
                break;
            }
        }

        var consumeItemReq = new ConsumeItemRequest()
        {
            ItemInstanceId = itemInstanceID,
            ConsumeCount = 1
        };
        PlayFabClientAPI.ConsumeItem(consumeItemReq, OnItemConsumed, OnUseError);
    }

    void OnUseError(PlayFabError e)
    {
        usedItemName = null;
    }

    void OnItemConsumed(ConsumeItemResult r)
    {
        foreach (ItemInstance item in inventory)
        {
            if (item.ItemInstanceId == r.ItemInstanceId)
            {
                usedItemName = item.DisplayName;
                Debug.Log("Item used: " + item.DisplayName);
                GetInventory();
                break;
            }
        }
    }
}
