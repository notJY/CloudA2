using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByContact : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    public int scoreValue;
    GameController gameController;

    private PlayerControl playerController;

    private void Awake()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
    }

    private void Start() {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if(gameControllerObject != null){
            gameController = gameControllerObject.GetComponent<GameController>();
        } 
        else{
            Debug.Log("GameController object not found");
        }
    }

    private void OnTriggerEnter(Collider other) {
        StartCoroutine(AsteroidCollision(other));
    }

    IEnumerator AsteroidCollision(Collider other)
    {
        if (other.tag != "Boundary")
        {
            Instantiate(explosion, transform.position, transform.rotation);
            if (other.tag == "Player")
            {
                playerController.OnItemUse("Revive");
                yield return new WaitUntil(()=> playerController.usedItemName != "");

                if ((playerController.usedItemName != "Revive") && (other != null))
                {
                    Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
                    Destroy(other.gameObject);
                    gameController.gameIsOver();
                }
                playerController.usedItemName = "";
            }
            gameController.addScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
