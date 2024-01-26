using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    private Rigidbody playerRb;

    private void Awake()
    {
         playerRb = gameObject.GetComponentInParent<Rigidbody>();
    }

    private void Update()
    {
        rb.position = new Vector3(playerRb.position.x, playerRb.position.y - 1.5f, playerRb.position.z - 3);
    }
}
