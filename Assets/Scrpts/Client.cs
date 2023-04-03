using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";

    public int port = 26950;
    public int ID = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exist, destroying object");
        }
    }

    private void Start()
    {
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {


        tcp = new TCP();
        udp = new UDP();

        InitializeClientData(); // this initiallises the client for connecting to the server

        isConnected = true; // tells the ser that the player is connected

        tcp.Connect(); // tells the tcp socket to connect to the server
                       // udp.Connect(port); 
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;

        private Packet recievedData;

        private byte[] recievedBuffer;

        public void Connect()
        {
            socket = new TcpClient // makes a new tcp client for the socket
            {
                ReceiveBufferSize = dataBufferSize, // sets the buffer size for the recieved and send
                SendBufferSize = dataBufferSize
            };

            recievedBuffer = new byte[dataBufferSize]; // sets the recieved buffer to have the a size of the data buffer field
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket); // makes the socket begin to connect based off of the instnace ip and port
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result); // ends the connect based on the IAsyncResult

            if (!socket.Connected) // if the socket is not connected
            {
                return; // exits the function so it doesn't continue and throw an error
            }

            stream = socket.GetStream(); // gets the stream from the current socket
            recievedData = new Packet(); // makes the recieved data =  a new packet
            stream.BeginRead(recievedBuffer, 0, dataBufferSize, RecievedCallback, null); // begins to read the packet with an offset of 0
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null) // checks that the socket is not null and has a value it can work with
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // begins to write to the packet with an offset of 0 
                }
            }
            catch (Exception exception) // if for whatever reason it can do what is in the try block
            {
                Debug.Log($"Error sending data to the server via TCP: {exception}"); // print the error message to the debugger
            }
        }

        private void RecievedCallback(IAsyncResult result)
        {
            try // trys to run the code 
            {
                int _byteLength = stream.EndRead(result); // makes the byte length equal whatever the IAsync result was

                if (_byteLength <= 0) // if the byte length was less than or equal to 0 it will disconnect and exit the function
                {
                    instance.Disconnect(); // calls the instances disconnect 
                    return; // exits out of the function 
                }
                byte[] newData = new byte[_byteLength]; // makes the recieved data equal the length of he data that was pulled from the packet
                Array.Copy(recievedBuffer, newData, _byteLength);// copies the data from one array to another


                recievedData.Reset(HandleData(newData)); // resets the packet if all the data was handled correctly
                stream.BeginRead(recievedBuffer, 0, dataBufferSize, RecievedCallback, null); // begins to the read the packet with an offset of 0

            }
            catch (Exception exception) // will run this section if for whatever reason it can not 
            {
                Disconnect(); // disconnects the client
                Debug.Log($"Error recieving data from the server via TCP: {exception}"); // print the error message to the debugger

            }
        }


        // prepares the data so it is used by the correct handler method
        private bool HandleData(byte[] nData)
        {
            int packetLength = 0;

            recievedData.SetBytes(nData);

            if (recievedData.UnreadLength() >= 4) // checks if there is still data to be read from the packet
            {

                packetLength = recievedData.ReadInt(); // reads how long the packet is 
                if (packetLength <= 0) // if it less than or equal to 0
                {
                    return true; // then the packet is finished reading
                }
            }
            while (packetLength > 0 && packetLength <= recievedData.UnreadLength()) // will loop whilst there is still data to be read from the packet
            {
                byte[] packetBytes = recievedData.ReadBytes(packetLength); // makes the packet bytes equalthe remaining bytes
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes)) // makes a new packet based off of the data from the packet bytes
                    {
                        int packetID = packet.ReadInt(); // sets the packet ID
                        packetHandlers[packetID](packet);
                    }
                });
                packetLength = 0; // sets the packet length to 0
                if (recievedData.UnreadLength() >= 4) // checks if there is more to read
                {
                    packetLength = recievedData.ReadInt(); // sets the packet length to be the size from the read int function of the packet
                    if (packetLength <= 0) // if it is 0 or less
                    {
                        return true; // then return true to say the packet is finised reading
                    }
                }
            }

            if (packetLength <= 1) // if the packet is 1 or less
            {
                return true; // then return true to say it is finished reading
            }
            Debug.Log("Handle Data Returned false");
            return false; // returns false to say there is more to be read
        }

        private void Disconnect()
        {
            instance.Disconnect(); // calls the Instance of the clients disconnect function

            stream = null; // sets the stream to null

            recievedData = null; // sets the recieved data to null
            recievedBuffer = null; // sets the buffer to null
            socket = null; // sets the socket to null
        }

    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(RecievedCallback, null);

            using (Packet newPacket = new Packet())
            {
                SendData(newPacket);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(instance.ID);
                if (socket != null)
                {
                    Debug.Log($"Player {instance.ID} sending UDP datagram to the server... ");
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending via UDP: {ex}");
            }
        }

        private void RecievedCallback(IAsyncResult result)
        {
            try
            {
                byte[] _data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(RecievedCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }
                HandleData(_data);
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }
        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }

    // this is used to initialize the clients 
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, ClientHandle.Welcome },
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
                { (int)ServerPackets.predictPosition,ClientHandle.PredictPosition},
                {(int)ServerPackets.despawnPlayer,ClientHandle.DespawnPlayer },

            };

        Debug.Log("Initialize packets");
    }
    private void Disconnect()
    {
        if (isConnected) // checks if it is currently connected 
        {
            isConnected = false; // sets the connected bool to false
            tcp.socket.Close(); // closes the sockets connection
            udp.socket.Close();


            Debug.Log("Disconnected from the server."); // prints a message to the debugger saying it is no longer connected
        }
    }
}
