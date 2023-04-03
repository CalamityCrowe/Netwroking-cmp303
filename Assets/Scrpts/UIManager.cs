using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;
    private void Awake()
    {
        if (instance == null) // checks if the instance is null
        {
            instance = this; // instanciates the instance of the object
        }
        else if (instance != this)  // checks that the instance does not equal the current instance
        {
            Debug.Log("Instance already exist, destroying object"); // prints a message to say it already exists
        }
    }

    public void ConnectToServer() 
    {
        startMenu.SetActive(false); // disables the start menu
        usernameField.interactable = false; // disables the userName field
        Client.instance.ConnectToServer(); // calls the clients function to connect to the server
    }
}
