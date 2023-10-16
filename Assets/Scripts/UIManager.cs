using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    public void StartMatch()
    {
        StartCoroutine(_APIManager.JoinMatch());
    }

    public void TriggerWaitingText()
    {
        ConnectionFailed.SetActive(true);
    }

    public void ConnectToServerSuccess()
    {
        ReadyButton.SetActive(false);
        LeaveQueue.SetActive(true);
        PlayerName.interactable = false;
    }

    public void LeaveQueueClick()
    {
        StartCoroutine(_APIManager.LeaveQueue());
    }

    public void LeftQueue()
    {
        StartGameBTN.interactable = false;
        ReadyButton.SetActive(true);
        LeaveQueue.SetActive(false);
        PlayerName.interactable = true;
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

    public void MatchFound()
    {
        StartGameBTN.interactable=true;
    }

    #endregion


    #region Game Menu Fields
    [SerializeField] TextMeshProUGUI _Question;
    [SerializeField] ButtonManager[] _Answers;
    private Dictionary<string, string> currentQuestion;
    #endregion

    #region Game Menu Functions

    public void UpdateQuestion(Dictionary<string,string> newQuestion)
    {
        currentQuestion = newQuestion;
    }

    public void UpdateQuestionUI()
    {
        _Question.text = currentQuestion["QuestionText"];
        for (int i = 0; i<4; i++)
        {
            _Answers[i].updateAnswer((i+1).ToString(),currentQuestion["Answer" + (i + 1)]);
        }
    
    }

    public bool SubmitAnswer(string AnswerID)
    {
        bool result = false;
        StartCoroutine(_APIManager.AnswerQuestion(AnswerID, (isCorrect) => {
            result = isCorrect; 
        }));
        return result;
    }


    #endregion


}
