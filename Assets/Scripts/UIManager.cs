using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField PlayerName;
    [SerializeField] APIManager _APIManager;
    [SerializeField] Button StartGameBTN;
    [SerializeField] GameObject ConnectionFailed;

    public void OnJoinGameButtonClicked()
    {
        StartCoroutine(_APIManager.JoinGame(PlayerName.text));
    }

    public void TriggerWaitingText()
    {
        ConnectionFailed.SetActive(true);
    }

    public void ConnectToServerSuccess()
    {
        StartGameBTN.interactable = true;
    }
    

}
