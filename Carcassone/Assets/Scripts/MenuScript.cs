using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Canvas canvas;
    private GameObject createGameButton, joinGameButton, settingsButton, quitGameButton;
    private GameObject joinGameText, joinGameInput, joinGameErrorText, joinGameStartButton, joinGameToMenuButton;
    private GameObject roomNameText, roomNameInput, roomNameErrorText, startGameButton, createGameToMenuButton;
    private GameObject volumeText, volumeSlider, settingsToMenuButton;
    private GameObject quitGameText, quitGameConfirmButton, quitGameCancelButton;
    // Start is called before the first frame update
    void Start()
    {
        createGameButton = canvas.transform.Find("createGameButton").gameObject;
        joinGameButton = canvas.transform.Find("joinGameButton").gameObject;
        settingsButton = canvas.transform.Find("settingsButton").gameObject;
        quitGameButton = canvas.transform.Find("quitGameButton").gameObject;

        roomNameText = canvas.transform.Find("roomNameText").gameObject;
        roomNameInput = canvas.transform.Find("roomNameInput").gameObject;
        roomNameErrorText = canvas.transform.Find("roomNameErrorText").gameObject;
        startGameButton = canvas.transform.Find("startGameButton").gameObject;
        createGameToMenuButton = canvas.transform.Find("createGameToMenuButton").gameObject;

        joinGameText = canvas.transform.Find("joinGameText").gameObject;
        joinGameInput = canvas.transform.Find("joinGameInput").gameObject;
        joinGameErrorText = canvas.transform.Find("joinGameErrorText").gameObject;
        joinGameStartButton = canvas.transform.Find("joinGameStartButton").gameObject;
        joinGameToMenuButton = canvas.transform.Find("joinGameToMenuButton").gameObject;

        volumeText = canvas.transform.Find("volumeText").gameObject;
        volumeSlider = canvas.transform.Find("volumeSlider").gameObject;
        settingsToMenuButton = canvas.transform.Find("settingsToMenuButton").gameObject;

        quitGameText = canvas.transform.Find("quitGameText").gameObject;
        quitGameConfirmButton = canvas.transform.Find("quitGameConfirmButton").gameObject;
        quitGameCancelButton = canvas.transform.Find("quitGameCancelButton").gameObject;

        onMenuEnter();
    }

    public void onMenuEnter()
    {
        roomNameText.SetActive(false);
        roomNameInput.SetActive(false);
        roomNameErrorText.SetActive(false);
        startGameButton.SetActive(false);
        createGameToMenuButton.SetActive(false);


        joinGameText.SetActive(false);
        joinGameInput.SetActive(false);
        joinGameErrorText.SetActive(false);
        joinGameStartButton.SetActive(false);
        joinGameToMenuButton.SetActive(false);


        volumeText.SetActive(false);
        volumeSlider.SetActive(false);
        settingsToMenuButton.SetActive(false);


        quitGameText.SetActive(false);
        quitGameConfirmButton.SetActive(false);
        quitGameCancelButton.SetActive(false);


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
        startGameButton.SetActive(false);
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
            startGameButton.SetActive(false);
        }
        else startGameButton.SetActive(true);
    }

    public void onStartGameButtonPress()
    {
        Debug.Log("let's go");
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
        joinGameStartButton.SetActive(false);
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
            joinGameStartButton.SetActive(false);
        }
        else joinGameStartButton.SetActive(true);
    }

    public void onJoinGameStartButtonPress()
    {
        Debug.Log("let's go");
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
}
