using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CollectScript : MonoBehaviour
{
    public static CollectScript instance;
    public bool isCollected = false;

    public int ID;

    // Start is called before the first frame update
    void Start()
    {
        ID = 0;
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCollected == true)
        {
            Debug.Log("AAAAAAAAAAAAAAAAAA");
            Destroy(gameObject);
        }
    }

}
