using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Canvases
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject Game;
    #endregion

    #region Main Menu Fields
    [SerializeField] TMP_InputField PlayerName;
    [SerializeField] APIManager _APIManager;
    [SerializeField] Button StartGameBTN;
    [SerializeField] GameObject ReadyButton;
    [SerializeField] GameObject LeaveQueue;
    [SerializeField] GameObject ConnectionFailed; 
    #endregion

    #region Main Menu Functions
    public void OnJoinGameButtonClicked()
    {
        StartCoroutine(_APIManager.JoinGame(PlayerName.text));
    }

    public void JoinGame()
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

    public void StartGame()
    {
        MainMenu.SetActive(false);
        Game.SetActive(true);
    }

    public void EndGame()
    {
        MainMenu.SetActive(true);
        Game.SetActive(false);
    }

    #endregion


    #region Game Menu Fields
    [SerializeField] TextMeshProUGUI _Question;
    [SerializeField] Button[] _Answers;
    #endregion

    #region Game Menu Functions


    #endregion


}
