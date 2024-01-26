using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

[System.Serializable]
public class Boundary{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerControl : MonoBehaviour {

    private Rigidbody playerRb;
    private AudioSource playerWeapon;
    public float speed;
    public float tiltMultiplier;
    public Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public Transform shotSpawn2;
    public float fireRate;

    private float nextFire;
    private CharacterSelection characterSelection;

    [SerializeField] GameObject shieldPrefab;
    public float shieldCooldown = 13f;
    private float shieldDuration = 10f;
    private float shieldTimer = 15f;
    private GameObject shield;
    private List<ItemInstance> inventory = new List<ItemInstance>();
    [HideInInspector]public string usedItemName = "";
    private string itemInstanceID = "";

    private void Awake()
    {
        GetInventory();
        
        for (int i = 0; i < StatDisplayManager.statList.Count; i++)
        {
            if (StatDisplayManager.statList[i].name == "Speed")
            {
                speed = float.Parse(StatDisplayManager.statList[i].value);
            }
            else if (StatDisplayManager.statList[i].name == "Fire Rate")
            {
                fireRate = float.Parse(StatDisplayManager.statList[i].value);
            }
        }
    }

    private void Start() {
        GameObject cSelectionObject = GameObject.FindWithTag("CharacterSelection");
        if (cSelectionObject != null) {
            characterSelection = cSelectionObject.GetComponent<CharacterSelection>();
        }

        playerRb = GetComponent<Rigidbody>();
        playerWeapon = GetComponent<AudioSource>();
    }

    private void Update() {
        if(Input.GetButton("Jump") && Time.time > nextFire){
            nextFire = Time.time + fireRate;
            if(characterSelection.getIndex() == 1){
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
                Instantiate(shot, shotSpawn2.position, shotSpawn2.rotation);
            }
            else{
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            }
            playerWeapon.Play();
        }

        if (Input.GetKeyDown(KeyCode.E) && (shieldTimer >= shieldCooldown))
        {
            shieldTimer = 0f;

            StartCoroutine(ActivateShield());
        }

        shieldTimer += Time.deltaTime;

        if (shield)
        {
            shield.transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        }
    }

    private void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        playerRb.velocity = new Vector3(moveHorizontal * speed, 0.0f, moveVertical * speed);

        playerRb.position = new Vector3(
            Mathf.Clamp(playerRb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(playerRb.position.z, boundary.zMin, boundary.zMax)
        );

        playerRb.rotation = Quaternion.Euler(0.0f, 0.0f, -playerRb.velocity.x * tiltMultiplier);
    }

    IEnumerator ActivateShield()
    {
        OnItemUse("Shield");
        yield return new WaitUntil(()=> usedItemName != "");
        if (usedItemName == "Shield")
        {
            shield = Instantiate(shieldPrefab, transform.localPosition, transform.localRotation * Quaternion.Euler(90, 0, 0),
                                 transform);
            yield return new WaitUntil(()=> shieldTimer >= shieldDuration);
            Destroy(shield);
        }
        usedItemName = "";
    }

    public void GetInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInventoryReceived, null);
    }

    void OnInventoryReceived(GetUserInventoryResult r)
    {
        inventory = r.Inventory;
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
        PlayFabClientAPI.ConsumeItem(consumeItemReq, OnItemConsumed, OnError);
    }

    void OnError(PlayFabError e)
    {
        Debug.Log(e.GenerateErrorReport());
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
