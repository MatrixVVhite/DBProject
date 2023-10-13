using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField PlayerName;
    [SerializeField] APIManager _APIManager;
    [SerializeField] Button StartGameBTN;
    [SerializeField] GameObject ReadyButton;
    [SerializeField] GameObject LeaveQueue;
    [SerializeField] GameObject ConnectionFailed;

    public void OnJoinGameButtonClicked()
    {
        StartCoroutine(_APIManager.JoinGame(PlayerName.text));
    }

    public void StartGame()
    {
        StartCoroutine(_APIManager.TryStartGame());
    }

    public void TriggerWaitingText()
    {
        ConnectionFailed.SetActive(true);
    }

    public void ConnectToServerSuccess()
    {
        StartGameBTN.interactable = true;
        ReadyButton.SetActive(false);
        LeaveQueue.SetActive(true);
        PlayerName.interactable = false;
    }

    public void LeaveQueueClick()
    {
        StartGameBTN.interactable = false;
        ReadyButton.SetActive(true);
        LeaveQueue.SetActive(false);
        PlayerName.interactable = true;
        StartCoroutine(_APIManager.LeaveGame());
    }
    

}
