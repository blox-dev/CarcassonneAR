using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Canvas canvas;
    private GameObject createGameButton, joinGameButton, quitGameButton;
    private GameObject roomNameText, roomNameInput, roomNameErrorText, startGameButton;
    // Start is called before the first frame update
    void Start()
    {
        createGameButton = canvas.transform.Find("createGameButton").gameObject;
        joinGameButton = canvas.transform.Find("joinGameButton").gameObject;
        quitGameButton = canvas.transform.Find("quitGameButton").gameObject;

        roomNameText = canvas.transform.Find("roomNameText").gameObject;
        roomNameInput = canvas.transform.Find("roomNameInput").gameObject;
        roomNameErrorText = canvas.transform.Find("roomNameErrorText").gameObject;
        startGameButton = canvas.transform.Find("startGameButton").gameObject;
    }

    public void onCreateGameButtonPress()
    {
        createGameButton.SetActive(false);
        joinGameButton.SetActive(false);
        quitGameButton.SetActive(false);

        roomNameText.SetActive(true);
        roomNameInput.SetActive(true);
        roomNameErrorText.SetActive(true);
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
}
