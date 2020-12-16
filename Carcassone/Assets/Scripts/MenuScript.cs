using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Text;

public class MenuScript : MonoBehaviourPunCallbacks
{
    public Canvas canvas;
    private GameObject createGameButton, joinGameButton, settingsButton, quitGameButton;
    private GameObject joinGameText, joinGameInput, joinGameErrorText, joinLobbyButton, joinGameToMenuButton;
    private GameObject roomNameText, roomNameInput, roomNameErrorText, startLobbyButton, createGameToMenuButton;
    private GameObject volumeText, volumeSlider, settingsToMenuButton;
    private GameObject quitGameText, quitGameConfirmButton, quitGameCancelButton;
    private GameObject lobbyRoomNameText, lobbyPlayerCountText, lobbyPlayerScrollView, startGameButton;

    private GameObject backgroundMusic;

    string gameVersion = "1.1";
    bool isConnecting;
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.PhotonServerSettings.DevRegion = "eu";
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.NickName = RandomString();
        if (isConnecting)
        {
            Debug.Log("Player connected to EU server");
            isConnecting = false;
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create room failed.");
        roomNameErrorText.GetComponent<Text>().text = message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join room failed.");
        joinGameErrorText.GetComponent<Text>().text = message;
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room succesfully.");
        roomNameText.SetActive(false);
        roomNameInput.SetActive(false);
        roomNameErrorText.SetActive(false);
        startLobbyButton.SetActive(false);
        createGameToMenuButton.SetActive(false);

        lobbyRoomNameText.SetActive(true);
        lobbyPlayerCountText.SetActive(true);
        lobbyPlayerScrollView.SetActive(true);
        startGameButton.SetActive(true);

        UpdatePlayers();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room succesfully.");
        joinGameText.SetActive(false);
        joinGameInput.SetActive(false);
        joinGameErrorText.SetActive(false);
        joinLobbyButton.SetActive(false);
        joinGameToMenuButton.SetActive(false);

        lobbyRoomNameText.SetActive(true);
        lobbyPlayerCountText.SetActive(true);
        lobbyPlayerScrollView.SetActive(true);
        //startGameButton.SetActive(true);

        UpdatePlayers();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdatePlayers();
    }

    public void UpdatePlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log(player.NickName);
        }
    }
    void Awake()
    {
        DontDestroyOnLoad(this);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        Connect();

        createGameButton = canvas.transform.Find("createGameButton").gameObject;
        joinGameButton = canvas.transform.Find("joinGameButton").gameObject;
        settingsButton = canvas.transform.Find("settingsButton").gameObject;
        quitGameButton = canvas.transform.Find("quitGameButton").gameObject;

        roomNameText = canvas.transform.Find("roomNameText").gameObject;
        roomNameInput = canvas.transform.Find("roomNameInput").gameObject;
        roomNameErrorText = canvas.transform.Find("roomNameErrorText").gameObject;
        startLobbyButton = canvas.transform.Find("startLobbyButton").gameObject;
        createGameToMenuButton = canvas.transform.Find("createGameToMenuButton").gameObject;

        joinGameText = canvas.transform.Find("joinGameText").gameObject;
        joinGameInput = canvas.transform.Find("joinGameInput").gameObject;
        joinGameErrorText = canvas.transform.Find("joinGameErrorText").gameObject;
        joinLobbyButton = canvas.transform.Find("joinLobbyButton").gameObject;
        joinGameToMenuButton = canvas.transform.Find("joinGameToMenuButton").gameObject;

        volumeText = canvas.transform.Find("volumeText").gameObject;
        volumeSlider = canvas.transform.Find("volumeSlider").gameObject;
        settingsToMenuButton = canvas.transform.Find("settingsToMenuButton").gameObject;

        quitGameText = canvas.transform.Find("quitGameText").gameObject;
        quitGameConfirmButton = canvas.transform.Find("quitGameConfirmButton").gameObject;
        quitGameCancelButton = canvas.transform.Find("quitGameCancelButton").gameObject;

        lobbyRoomNameText = canvas.transform.Find("lobbyRoomNameText").gameObject;
        lobbyPlayerCountText = canvas.transform.Find("lobbyPlayerCountText").gameObject;
        lobbyPlayerScrollView = canvas.transform.Find("lobbyPlayerScrollView").gameObject;
        startGameButton = canvas.transform.Find("startGameButton").gameObject;

        backgroundMusic = GameObject.Find("backgroundMusic");
        AudioSource audio = backgroundMusic.GetComponent<AudioSource>();
        audio.Play();

        onMenuEnter();
    }

    public void onMenuEnter()
    {
        roomNameText.SetActive(false);
        roomNameInput.SetActive(false);
        roomNameErrorText.SetActive(false);
        startLobbyButton.SetActive(false);
        createGameToMenuButton.SetActive(false);


        joinGameText.SetActive(false);
        joinGameInput.SetActive(false);
        joinGameErrorText.SetActive(false);
        joinLobbyButton.SetActive(false);
        joinGameToMenuButton.SetActive(false);


        volumeText.SetActive(false);
        volumeSlider.SetActive(false);
        settingsToMenuButton.SetActive(false);


        quitGameText.SetActive(false);
        quitGameConfirmButton.SetActive(false);
        quitGameCancelButton.SetActive(false);


        lobbyRoomNameText.SetActive(false);
        lobbyPlayerCountText.SetActive(false);
        lobbyPlayerScrollView.SetActive(false);
        startGameButton.SetActive(false);

        createGameButton.SetActive(true);
        joinGameButton.SetActive(true);
        settingsButton.SetActive(true);
        quitGameButton.SetActive(true);


    }

    public void onCreateGameButtonPress()
    {
        createGameButton.SetActive(false);
        joinGameButton.SetActive(false);
        settingsButton.SetActive(false);
        quitGameButton.SetActive(false);

        roomNameText.SetActive(true);
        roomNameInput.SetActive(true);
        roomNameErrorText.SetActive(true);
        createGameToMenuButton.SetActive(true);
        startLobbyButton.SetActive(false);
    }

    public void onRoomNameChange()
    {
        InputField inputValue = roomNameInput.GetComponent<InputField>();
        Text errorText = roomNameErrorText.GetComponent<Text>();
        if (inputValue.text.Length >= 16 || inputValue.text.Length <= 2)
            errorText.text = "ROOM NAME MUST HAVE 3-16 CHARACTERS";
        else errorText.text = "";
    }

    public void onRoomNameValidationFail()
    {
        InputField inputValue = roomNameInput.GetComponent<InputField>();
        Text errorText = roomNameErrorText.GetComponent<Text>();
        if (errorText.text != "")
        {
            inputValue.text = "";
            startLobbyButton.SetActive(false);
        }
        else startLobbyButton.SetActive(true);
    }

    public void onStartLobbyButtonPress()
    {
        string roomName = roomNameInput.GetComponent<InputField>().text;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 5 }); ;
    }

    public void onBackToMenuButtonPress()
    {
        onMenuEnter();
    }

    public void onJoinGameButtonPress()
    {
        createGameButton.SetActive(false);
        joinGameButton.SetActive(false);
        settingsButton.SetActive(false);
        quitGameButton.SetActive(false);

        joinGameText.SetActive(true);
        joinGameInput.SetActive(true);
        joinGameErrorText.SetActive(true);
        joinLobbyButton.SetActive(false);
        joinGameToMenuButton.SetActive(true);
    }

    public void onJoinRoomNameChange()
    {
        InputField inputValue = joinGameInput.GetComponent<InputField>();
        Text errorText = joinGameErrorText.GetComponent<Text>();
        if (inputValue.text.Length >= 16 || inputValue.text.Length <= 2)
            errorText.text = "ROOM NAME MUST HAVE 3-16 CHARACTERS";
        else errorText.text = "";
    }

    public void onJoinRoomNameValidationFail()
    {
        InputField inputValue = joinGameInput.GetComponent<InputField>();
        Text errorText = joinGameErrorText.GetComponent<Text>();
        if (errorText.text != "")
        {
            inputValue.text = "";
            joinLobbyButton.SetActive(false);
        }
        else joinLobbyButton.SetActive(true);
    }

    public void onJoinLobbyButtonPress()
    {
        string roomName = joinGameInput.GetComponent<InputField>().text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void onSettingsButtonPress()
    {
        createGameButton.SetActive(false);
        joinGameButton.SetActive(false);
        settingsButton.SetActive(false);
        quitGameButton.SetActive(false);

        volumeText.SetActive(true);
        volumeSlider.SetActive(true);
        settingsToMenuButton.SetActive(true);

    }

    public void onQuitGameButtonPress()
    {
        createGameButton.SetActive(false);
        joinGameButton.SetActive(false);
        settingsButton.SetActive(false);
        quitGameButton.SetActive(false);

        quitGameText.SetActive(true);
        quitGameConfirmButton.SetActive(true);
        quitGameCancelButton.SetActive(true);
    }

    public void onQuitGameConfirmButtonPress()
    {
        Application.Quit(0);
    }

    public void onStartGameButtonPress()
    {
        Debug.Log("Joining game.");
        if(PhotonNetwork.IsMasterClient)
{
            PhotonNetwork.LoadLevel("yes");
        }
    }
    private string RandomString()
    {
        const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
        int charAmount = 10;
        StringBuilder str = new StringBuilder();

        for (int i = 0; i<charAmount; i++)
        {
            str.Append(glyphs[Random.Range(0, glyphs.Length)]);
        }

        return str.ToString();
    }
}
