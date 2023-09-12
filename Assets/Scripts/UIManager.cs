using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_InputField PlayerName;
    [SerializeField] APIManager _APIManager;
    [SerializeField] TMP_Text WaitingForPlayer;

    public void OnJoinGameButtonClicked()
    {
        _APIManager.JoinGame(PlayerName.text);
    }

    public void TriggerWaitingText()
    {
        WaitingForPlayer.Enabled;
    }
    

}
