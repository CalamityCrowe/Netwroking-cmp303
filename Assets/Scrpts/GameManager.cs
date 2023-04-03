using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance; 

    public static Dictionary <int, PlayerManager> currentPlayers = new Dictionary<int, PlayerManager> (); // a dictionary holding all the players based off the ID they have assigned to them
    public List<GameObject> collectables;

    public GameObject collectPrefab; 

    public GameObject PlayerPrefab; // the prefab used for the player
    public GameObject EnemyPrefab; // prefab used for the enemy players

    private void Awake() // runs when the game launches
    {
        if (instance == null) // sets the instance to null
        {
            instance = this; // sets the current instance of the object to this
        }
        else if (instance != this) // checks if the instance does not equal this
        {
            Debug.Log("Instance already exist, destroying object"); // says it already exists in another context
        }
    }

    public void spawnPlayer(int newID, string newUsername, Vector2 newPosition) 
    {
        GameObject newPlayer; 
        if (newID == Client.instance.ID) // checks if the ID passed in to the function equals the current instances
        {
            newPlayer = Instantiate(PlayerPrefab, newPosition, new Quaternion()); // assigns the player prefab to the new player instance
        }
        else
        {
            newPlayer = Instantiate(EnemyPrefab, newPosition, new Quaternion()); // other wise assigns the enemy prefab to the insance
        }
        newPlayer.GetComponent<PlayerManager>().ID = newID; // sets the id to be that of the new ID
        newPlayer.GetComponent<PlayerManager>().username = newUsername; // sets the username to be that of the new username passed in

        currentPlayers.Add(newID, newPlayer.GetComponent<PlayerManager>()); //adds the current instance to the dictionary of players
        currentPlayers[currentPlayers.Count].isConnected = true; ;
    }
    public void spawnCollectable(float posX,float posY) 
    {
        collectables.Add(collectPrefab);
        collectables[collectables.Count - 1].GetComponent<CollectScript>().ID = collectables.Count - 1;
        collectables[collectables.Count - 1].gameObject.transform.position = new Vector3(posX, posY, 0); 
    }

    public void Update()
    {
        for (int i = 1; i <= currentPlayers.Count - 1; ++i) 
        {
            if (currentPlayers[i].isConnected == false) 
            {
                Destroy(currentPlayers[i]);
            }
        }

    }
}
