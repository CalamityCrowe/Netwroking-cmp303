using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int ID;
    public string username;
    public bool isConnected;

    private int score; 

    float timer = 0;

    public List<Vector2> predict_Pos = new List<Vector2>();
    public List<float> lastTime = new List<float>();

    public void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            predict_Pos.Add(new Vector2(0, 0));
            lastTime.Add(i);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (isConnected == false)
        {
            Destroy(this.gameObject);
        }
        Vector2 pos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        ClientSend.PlayerPosition(pos, timer); // sends the players current position

        ScoreManager.instance.setScore(score); 

        Debug.Log("Player ID:" + ID);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        int collisionLayer = collision.gameObject.layer;

        if (collisionLayer == 6) 
        {
            score += 1;
            collision.gameObject.GetComponent<CollectScript>().isCollected = true;
        }
    }

    public void predictPosition()
    {
        float predictedX = -1;
        float predictedY = -1;

        float speedX, speedY;

        float distanceX = predict_Pos[0].x - predict_Pos[1].x;
        float distanceY = predict_Pos[0].y - predict_Pos[1].y;

        float time = Convert.ToSingle(lastTime[0] - lastTime[1]);
        if (time == 0) { time = Time.deltaTime;  }

        speedX = (float)(distanceX / time);
        speedY = (float)(distanceY / time);

        //Debug.Log("GATE TIME:" +System.DateTime.Now.Millisecond); 
        //Debug.Log("Last Time:" + lastTime[1]); 


        float messageTime = Convert.ToSingle(Time.deltaTime - lastTime[1]);
        //Debug.Log("Message Time:" + messageTime); 

        if (messageTime < 0)
        {
            messageTime *= -1;
        }

        float displacementX = (float)speedX * time;
        float displacementY = (float)speedY * time;


        predictedX = (float)(predict_Pos[1].x + displacementX);
        predictedY = (float)(predict_Pos[1].y + displacementY);

        transform.position = new Vector3(predictedX, predictedY, 0);
        //Debug.Log("ID: " + ID +" predicted " + predictedX + " , " + predictedY);


        //Debug.Log("Predict Position: " + predict_Pos[1]); 


        predict_Pos.RemoveAt(0);
        lastTime.RemoveAt(0);
    }

}
