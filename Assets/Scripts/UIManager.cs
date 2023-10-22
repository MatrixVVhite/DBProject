using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
	#region CORE_OBJECTS
	[SerializeField] private GameManager _gameManager;
	[SerializeField] private APIManager _APIManager;
	#endregion

#if UNITY_EDITOR
	private void OnValidate()
	{
		_playerNameInputField = _registration.GetComponentInChildren<TMP_InputField>();
		/*SetSingleOnClickListener(_connectButton, OnConnectButtonClicked);
		SetSingleOnClickListener(_joinQueueButton, OnJoinQueueButtonClicked);
		SetSingleOnClickListener(_startMatchButton, OnStartMatchButtonClicked);
		SetSingleOnClickListener(_exitGameButton, OnExitGameButtonClicked);
		SetSingleOnClickListener(_disconnectButton, OnDisconnectButtonClicked);
		SetSingleOnClickListener(_leaveQueueButton, OnLeaveQueueButtonClicked);*/
	}

	private static void SetSingleOnClickListener(Button button, UnityAction call)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(call);
	}
#endif

	#region MENU_MENU
	#region CANVASES
	[SerializeField] private GameObject _mainMenuUI;
	[SerializeField] private GameObject _gameUI;
	#endregion

	#region ELEMENTS
	[SerializeField] private GameObject _registration;
	[SerializeField] private TMP_InputField _playerNameInputField;
	[SerializeField] private Button _connectButton;
	[SerializeField] private Button _joinQueueButton;
	[SerializeField] private Button _startMatchButton;
	[SerializeField] private Button _exitGameButton;
	[SerializeField] private Button _disconnectButton;
	[SerializeField] private Button _leaveQueueButton;
	[SerializeField] private GameObject _connectionFailed;
	#endregion

	#region FUNCTIONS
	#region BUTTONS
	public void OnConnectButtonClicked()
	{
		StartCoroutine(_APIManager.ConnectToServer(_playerNameInputField.text));
	}

	public void OnJoinQueueButtonClicked()
	{
		StartCoroutine(_APIManager.SubmitTicket());
	}

	public void OnStartMatchButtonClicked()
	{
		StartCoroutine(_APIManager.JoinMatch());
	}

	public void OnExitGameButtonClicked()
	{
		Application.Quit();
	}

	public void OnDisconnectButtonClicked()
	{
		StartCoroutine(_APIManager.DisconnectFromServer());
	}

	public void OnLeaveQueueButtonClicked()
	{
		StartCoroutine(_APIManager.RevokeTicket());
	}
	#endregion

	#region CALLBACKS
	public void OnConnectToServerSuccess()
	{
		_registration.SetActive(false);
		_connectButton.gameObject.SetActive(false);
		_exitGameButton.gameObject.SetActive(false);
		_joinQueueButton.gameObject.SetActive(true);
		_disconnectButton.gameObject.SetActive(true);
	}

	public void OnDisconnectFromServerSuccess()
	{
		_registration.SetActive(true);
		_connectButton.gameObject.SetActive(true);
		_exitGameButton.gameObject.SetActive(true);
		_joinQueueButton.gameObject.SetActive(false);
		_disconnectButton.gameObject.SetActive(false);
	}

	public void OnJoinQueueSuccess()
	{
		_joinQueueButton.gameObject.SetActive(true);
		_joinQueueButton.interactable = false;
		_leaveQueueButton.gameObject.SetActive(true);
		_disconnectButton.gameObject.SetActive(false);
	}

	public void OnLeftQueueSuccess()
	{
		_joinQueueButton.gameObject.SetActive(true);
		_joinQueueButton.interactable = true;
		_leaveQueueButton.gameObject.SetActive(false);
		_disconnectButton.gameObject.SetActive(true);
		_connectButton.gameObject.SetActive(false);
		_startMatchButton.gameObject.SetActive(false);
	}

	public void OnMatchFound()
	{
		_joinQueueButton.gameObject.SetActive(false);
		_startMatchButton.gameObject.SetActive(true);
	}

	public void OnStartGame()
	{
		_mainMenuUI.SetActive(false);
		_gameUI.SetActive(true);
		ResetGameUI();
	}
	#endregion

	public void TriggerWaitingText()
	{
		_connectionFailed.SetActive(true);
	}
	#endregion
	#endregion

	#region GAME
	#region FIELDS
	[SerializeField] TextMeshProUGUI _question;
	[SerializeField] ButtonManager[] _answers;
	[SerializeField] TextMeshProUGUI[] _playerStats;
	[SerializeField] GameObject _endGameScreen;
	[SerializeField] TextMeshProUGUI _finalMessage;
	[SerializeField] Button _exitMatchEnd;
	private Dictionary<string, string> _currentQuestion;
	#endregion

	#region FUNCTIONS
	public void ResetGameUI()
	{
		_question.text = string.Empty;
		_endGameScreen.SetActive(false);
		_finalMessage.text = string.Empty;
		_currentQuestion = new Dictionary<string, string>();
	}

	public void UpdateQuestion(Dictionary<string, string> newQuestion)
	{
		_currentQuestion = newQuestion;
	}

	public void UpdateQuestionUI()
	{
		try
		{
			_question.text = _currentQuestion["QuestionText"];
			for (int i = 0; i < 4; i++)
				_answers[i].UpdateAnswer((i + 1).ToString(), _currentQuestion["Answer" + (i + 1)]);
		}
		catch (KeyNotFoundException)
		{
			return;
		}
	}

	public IEnumerator SubmitAnswer(string AnswerID, ButtonManager button)
	{
		yield return StartCoroutine(_APIManager.AnswerQuestion(AnswerID, (isCorrect) => {
			button.ColorResponse(isCorrect); 
		}));
		yield return new WaitForSeconds(2);
		_gameManager.LoadQuestion();
	}

	public void UpdateScores(string yourScore, string otherScore, string yourQuestionsLeft, string otherQuestionsLeft)
	{
		_playerStats[0].text = "Your Score: \n" + yourScore;
		_playerStats[1].text = "Opponent's Score: \n" + otherScore;
		_playerStats[2].text = "Questions Left: \n" + yourQuestionsLeft;
		_playerStats[3].text = "Questions Left: \n" + otherQuestionsLeft;
	}

	public void LoadEndScreen()
	{
		_endGameScreen.SetActive(true);
	}

	public void UpdateEndMessage(string message)
	{
		_exitMatchEnd.interactable = true;
		_finalMessage.text = message;
	}

	public void OnExitMatchButtonClicked()
	{
		StartCoroutine(_APIManager.LeaveMatch());
	}

	public void OnExitMatchSuccessful()
	{
		_mainMenuUI.SetActive(true);
		_gameUI.SetActive(false);
		OnLeftQueueSuccess();
	}
	#endregion
	#endregion
}
