using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    [SerializeField] GameObject _StartHostGO;
    [SerializeField] TMP_Dropdown _BotDropdown;

    [Space]

    [SerializeField] GameObject _ConnectServerGO;
    [SerializeField] TMP_InputField _IPField;
    [SerializeField] TMP_InputField _PortField;

    [SerializeField] TMP_InputField _NickField;
    [SerializeField] Toggle _OrientToggle;

    void Start()
    {
        Cursor.visible = true;
        
        if (Application.platform == RuntimePlatform.WindowsServer)
        {
            Server.ServerState = 2;
            Server.NumberOfBots = 4;
            Server.ServerIP = "0.0.0.0";
            Debug.Log("Starting the server..");
            SceneManager.LoadScene("Dust", LoadSceneMode.Single);
            return;
        }

        _OrientToggle.isOn = Player.AutoOrientation;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _ConnectServerGO.SetActive(false);
            _StartHostGO.SetActive(false);
        }
    }

    public void ShowHosting()
    {
        _ConnectServerGO.SetActive(false);
        _StartHostGO.SetActive(true);
    }

    public void ShowConnection()
    {
        _ConnectServerGO.SetActive(true);
        _StartHostGO.SetActive(false);
    }

    public void Connect()
    {
        Server.ServerState = 0;
        Server.Nickname = _NickField.text;
        Server.ServerIP = _IPField.text;
        Server.ServerPort = (ushort)Convert.ToInt16(_PortField.text);

        SceneManager.LoadScene("Dust", LoadSceneMode.Single);
    }

    public void StartHost()
    {

        if (_BotDropdown.value == 0) Server.NumberOfBots = 0;
        else
        {
            Server.NumberOfBots = (byte)math.pow(2, _BotDropdown.value);
        }

        Server.ServerState = 1;
        Server.Nickname = _NickField.text;
        SceneManager.LoadScene("Dust", LoadSceneMode.Single);
    }

    public void ToggleAutoOrient()
    {
        Player.AutoOrientation = _OrientToggle.isOn;
    }
    public void Quit()
    {
        Application.Quit();
    }

}
