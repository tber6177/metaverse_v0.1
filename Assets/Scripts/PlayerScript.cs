using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public static PlayerScript instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("Created a second instance of PlayerScript. destroying.");
            Destroy(gameObject);
        }
    }

 
}
