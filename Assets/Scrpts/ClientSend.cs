using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet); //  sends the data in the packet
    }

    private static void SendUDPData(Packet packet) 
    {
        packet.WriteLength(); 
        Client.instance.udp.SendData(packet);
    }

    #region packets

    public static void WelcomRecieved()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.ID); // writes the clients ID
            packet.Write(UIManager.instance.usernameField.text); // writes the clients username

            SendTCPData(packet);  // sends the packet via tcp

        }
    }

    public static void PlayerMovement(bool[] newInputs)
    {
        using (Packet newPacket = new Packet((int)ClientPackets.playerMovement))
        {
            newPacket.Write(newInputs.Length); // writes how many keys are in the movements keys
            foreach (bool key in newInputs) // loops through all the keys for the players movements
            {
                newPacket.Write(key); // writes all the keys data to the packet
            }
            SendUDPData(newPacket);
        }
    }

    public static void PlayerPosition(Vector2 position,float time)
    {
        using (Packet newPacket = new Packet((int)ClientPackets.playerPosition))
        {
            newPacket.Write(position);
            newPacket.Write(time);
            SendUDPData(newPacket);
        }
    }



    #endregion
}
