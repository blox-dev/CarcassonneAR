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
    public GameObject playerUIPrefab;
    private GameObject createGameButton, joinGameButton, settingsButton, quitGameButton;
    private GameObject joinGameText, joinGameInput, joinGameErrorText, joinLobbyButton, joinGameToMenuButton, joinRandomRoomButton;
    private GameObject roomNameText, roomNameInput, roomNameErrorText, startLobbyButton, createGameToMenuButton;
    private GameObject volumeText, volumeSlider, nickNameText, nickNameInput, settingsToMenuButton;
    private GameObject quitGameText, quitGameConfirmButton, quitGameCancelButton;
    private GameObject lobbyRoomNameText, lobbyPlayerCountText, lobbyPlayerScrollView, startGameButton, content, lobbyToMenuButton;

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
        if (PlayerPrefs.HasKey("nickNamePref"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("nickNamePref");
        }
        else
        {
            string nick = RandomString("Guest");
            PhotonNetwork.NickName = nick;
            PlayerPrefs.SetString("nickNamePref", nick);
        }

        nickNameInput.GetComponent<InputField>().text = PhotonNetwork.NickName;

        if (isConnecting)
        {
            Debug.Log("Player connected to EU server");
            isConnecting = false;
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(RandomString("Room"), new RoomOptions { MaxPlayers = 5 });
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
        joinRandomRoomButton.SetActive(false);

        lobbyRoomNameText.SetActive(true);
        lobbyPlayerCountText.SetActive(true);
        lobbyPlayerScrollView.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }

        lobbyToMenuButton.SetActive(true);

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
        joinRandomRoomButton.SetActive(false);

        lobbyRoomNameText.SetActive(true);
        lobbyPlayerCountText.SetActive(true);
        lobbyPlayerScrollView.SetActive(true);
        
        if(PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }

        lobbyToMenuButton.SetActive(true);

        UpdatePlayers();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdatePlayers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("PLayer joined room");
        UpdatePlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("PLayer left room");
        UpdatePlayers();
    }

    public void UpdatePlayers()
    {
        lobbyPlayerCountText.GetComponent<Text>().text = PhotonNetwork.PlayerList.Length.ToString() + "/5 players connected.";
        lobbyRoomNameText.GetComponent<Text>().text = PhotonNetwork.CurrentRoom.Name;

        var children = new List<GameObject>();
        foreach (Transform child in content.transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject clone = Instantiate(playerUIPrefab, content.transform);
            clone.GetComponent<Text>().text = player.NickName;
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
        joinRandomRoomButton = canvas.transform.Find("joinRandomLobbyButton").gameObject;

        volumeText = canvas.transform.Find("volumeText").gameObject;
        volumeSlider = canvas.transform.Find("volumeSlider").gameObject;
        settingsToMenuButton = canvas.transform.Find("settingsToMenuButton").gameObject;
        nickNameText = canvas.transform.Find("nickNameText").gameObject;
        nickNameInput = canvas.transform.Find("nickNameInput").gameObject;

        quitGameText = canvas.transform.Find("quitGameText").gameObject;
        quitGameConfirmButton = canvas.transform.Find("quitGameConfirmButton").gameObject;
        quitGameCancelButton = canvas.transform.Find("quitGameCancelButton").gameObject;

        lobbyRoomNameText = canvas.transform.Find("lobbyRoomNameText").gameObject;
        lobbyPlayerCountText = canvas.transform.Find("lobbyPlayerCountText").gameObject;
        lobbyPlayerScrollView = canvas.transform.Find("lobbyPlayerScrollView").gameObject;
        startGameButton = canvas.transform.Find("startGameButton").gameObject;
        content = lobbyPlayerScrollView.transform.GetChild(0).GetChild(0).gameObject;
        lobbyToMenuButton = canvas.transform.Find("lobbyToMenuButton").gameObject;

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
        joinRandomRoomButton.SetActive(false);


        volumeText.SetActive(false);
        volumeSlider.SetActive(false);
        nickNameText.SetActive(false);
        nickNameInput.SetActive(false);
        settingsToMenuButton.SetActive(false);


        quitGameText.SetActive(false);
        quitGameConfirmButton.SetActive(false);
        quitGameCancelButton.SetActive(false);


        lobbyRoomNameText.SetActive(false);
        lobbyPlayerCountText.SetActive(false);
        lobbyPlayerScrollView.SetActive(false);
        startGameButton.SetActive(false);
        lobbyToMenuButton.SetActive(false);

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
        joinRandomRoomButton.SetActive(true);
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

    public void onJoinRandomRoomButtonPress()
    {
        PhotonNetwork.JoinRandomRoom();
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
        nickNameText.SetActive(true);
        nickNameInput.SetActive(true);
        settingsToMenuButton.SetActive(true);

    }

    public void onNickNameChange()
    {
        InputField inputValue = nickNameInput.GetComponent<InputField>();
        if(inputValue.text != "")
        {
            PhotonNetwork.NickName = inputValue.text;
            PlayerPrefs.SetString("nickNamePref", inputValue.text);
        }
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

    public void onLobbyToMenuButtonPress()
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            Debug.Log("Leaving room " + PhotonNetwork.CurrentRoom.Name);
            PhotonNetwork.LeaveRoom();
        }
        onMenuEnter();
    }

    public void onStartGameButtonPress()
    {
        Debug.Log("Joining game.");
        if(PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length >= 1)
        {
            PhotonNetwork.LoadLevel("yes");
        }
        else
        {
            Debug.Log("Failed joining room.");
        }
    }
    private string RandomString(string prefix)
    {
        int charAmount = 10;
        StringBuilder str = new StringBuilder(prefix);

        for (int i = 0; i<charAmount; i++)
        {
            str.Append(Random.Range(0,10).ToString());
        }

        return str.ToString();
    }
}