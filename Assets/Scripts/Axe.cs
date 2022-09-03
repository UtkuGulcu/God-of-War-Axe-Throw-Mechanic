using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour
{

    #region public variables

    [SerializeField] BoxCollider colliderPlayer;
    public bool axeIsStuck;

    #endregion

    #region public references



    #endregion

    #region private variables

    BoxCollider colliderAxe;

    #endregion

    #region private references

    Player playerScript;
    Rigidbody rbAxe;

    #endregion

    private void Awake()
    {
        colliderAxe = GetComponent<BoxCollider>();
        playerScript = FindObjectOfType<Player>();
        rbAxe = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Physics.IgnoreCollision(colliderAxe, colliderPlayer, true);
        axeIsStuck = false;
    }

    void Update()
    {
        if (playerScript.isRecalling)
        {
            transform.rotation *= Quaternion.Euler(8, 0, 8);
        }

        else if (!playerScript.axeIsOnHand && !axeIsStuck)
        {
            transform.rotation *= Quaternion.Euler(0, 0, 10);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        rbAxe.isKinematic = true;
        axeIsStuck = true;
    }
}
