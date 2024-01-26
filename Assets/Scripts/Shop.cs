using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] TMP_Text itemBoughtTxt, itemName, itemDesc, itemPrice;
    public Sprite[] shopItemImages;
    [SerializeField] Image currentItemImage;
    [SerializeField] UIManager uiManager;
    [SerializeField] Inventory inventoryManager;
    private bool isAwake;
    [HideInInspector]public List<CatalogItem> shopItems = new List<CatalogItem>();
    private int currentItem = 0;

    //Apparently Awake doesn't work with OnEnable, so OnEnable only
    private void OnEnable()
    {
        currentItem = 0;
        if (!isAwake)
        {
            StartCoroutine(InitShop());
        }
        else
        {
            RefreshShop();
        }
    }

    IEnumerator InitShop()
    {
        GetShopItems();
        yield return new WaitUntil(()=> shopItems.Count != 0);
        RefreshShop();
        isAwake = true;

        //GameObject scale starts at (0, 0, 0) so it can be hidden and initialize variables at the same time
        transform.localScale = new Vector3(1, 1, 1);

        gameObject.SetActive(false);
    }

    void RefreshShop()
    {
        itemBoughtTxt.text = "";
        itemName.text = shopItems[currentItem].DisplayName;
        itemDesc.text = shopItems[currentItem].Description;
        itemPrice.text = "Price: " + shopItems[currentItem].VirtualCurrencyPrices["GD"].ToString() + " Gold";
        currentItemImage.sprite = shopItemImages[currentItem];
    }

    void OnError(PlayFabError e)
    {
        Debug.Log("Error: " + e.GenerateErrorReport());
    }

    void GetShopItems()
    {
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnShopItemsReceived, OnError);
    }

    void OnShopItemsReceived(GetCatalogItemsResult r)
    {
        shopItems = r.Catalog;
    }

    public void OnPreviousItem()
    {
        if (currentItem > 0)
        {
            currentItem--;
        }
        RefreshShop();
    }

    public void OnNextItem()
    {
        if (currentItem < shopItems.Count - 1)
        {
            currentItem++;
        }
        RefreshShop();
    }

    public void OnBuyItem()
    {
        var purchaseReq = new PurchaseItemRequest()
        {
            ItemId = shopItems[currentItem].ItemId,
            VirtualCurrency = "GD",
            Price = (int)shopItems[currentItem].VirtualCurrencyPrices["GD"]
        };
        PlayFabClientAPI.PurchaseItem(purchaseReq, OnPurchaseSucc, OnError);
    }

    void OnPurchaseSucc(PurchaseItemResult r)
    {
        uiManager.GetCurrency();
        inventoryManager.GetInventory();
        itemBoughtTxt.text = r.Items[0].DisplayName + " Bought!";
        inventoryManager.isUpdated = false;
    }

}
