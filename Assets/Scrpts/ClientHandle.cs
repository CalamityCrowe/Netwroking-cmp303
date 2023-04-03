using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    // prints a message to the screen to say that the client has connected to the server
    public static void Welcome(Packet newPacket)
    {
        string message = newPacket.ReadString();
        int ID = newPacket.ReadInt();

        Debug.Log($"Message from the server:{message}");
        Debug.Log($"ID from server:{ID}");


        Client.instance.ID = ID; // assigns the clients current ID
        ClientSend.WelcomRecieved(); // sends a message back saying they have recieved the message that they have connected


        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    // tells the server that the player has spawned in at said location
    public static void SpawnPlayer(Packet newPacket)
    {
        // sets the ID field from the player
        int ID = newPacket.ReadInt();
        string username = newPacket.ReadString(); // sets the username from the packet
        Vector2 position = newPacket.ReadVector2(); //sets the players position based on what was sent from the packet
        Debug.Log(ID); 
        GameManager.instance.spawnPlayer(ID, username, position); // tells the game manager to spawn the player based off of the info from the packet 
    }

    public static void SpawnCollect(Packet newPacket) 
    {
        float posX = newPacket.ReadFloat();
        float posY = newPacket.ReadFloat();
        GameManager.instance.spawnCollectable(posX, posY); 
    }

    // updates the players position based off of the packet recieved from the server
    public static void PlayerPosition(Packet newPacket)
    {
        int ID = newPacket.ReadInt(); // gets the ID from the packet
        Vector2 newPosition = newPacket.ReadVector2(); // gets the position from the packet

        if (GameManager.currentPlayers.TryGetValue(ID, out PlayerManager player)) // tries to get the current player based off of the id from the packet
        {
            player.transform.position = newPosition; // updates the players position based off of this
        }
    }
    public static void DespawnPlayer(Packet newPacket)
    {
        int ID = newPacket.ReadInt();

        bool isConnected = newPacket.ReadBool();
        Destroy(GameManager.currentPlayers[ID].gameObject);
        GameManager.currentPlayers.Remove(ID);
    }

    public static void RemoveCollect(Packet newPacket) 
    {
        int id = newPacket.ReadInt();
        Destroy(GameManager.instance.collectables[id]);
    }

    public static void PredictPosition(Packet newPacket)
    {
        int ID = newPacket.ReadInt();
        float x = newPacket.ReadFloat();
        float y = newPacket.ReadFloat();

        Debug.Log("Packet ID: " + ID);

        float t = newPacket.ReadFloat(); // reads the time

        Vector2 pos = new Vector2(x, y);
        Debug.Log("POS: " + pos);


        GameManager.currentPlayers[ID].predict_Pos.Add(pos);
        GameManager.currentPlayers[ID].lastTime.Add(t);
        if (GameManager.currentPlayers[ID].predict_Pos.Count >= 3)
        {
            GameManager.currentPlayers[ID].predictPosition();
        }

    }
}
