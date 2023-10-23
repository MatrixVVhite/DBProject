using System.Collections;
using UnityEngine;
using UI;

public class GameManager : MonoBehaviour
{
	public const int QUESTIONS_PER_MATCH = 10;
	public const int MAX_SCORE_PER_QUESTION = 10;
	public const int MAX_SCORE_PER_MATCH = QUESTIONS_PER_MATCH * MAX_SCORE_PER_QUESTION;
	public const int GAME_FINISHED_QUESTION_COUNT = QUESTIONS_PER_MATCH + 1;
	[SerializeField] private InGameMenu _inGameMenu;
	private string _yourName;
	private string _otherName;
	private int _yourScore = 0;
	private int _otherScore = 0;
	private int _yourQuestionsLeft = QUESTIONS_PER_MATCH;
	private int _otherQuestionsLeft = QUESTIONS_PER_MATCH;
	private float _currentAnswerStartTime = 0f;

	public static GameManager Instance { get; private set; }
	private bool GameRunning => YouHaveQuestionsLeft | OtherHaveQuestionsLeft;
	private bool WaitingForEnd => !YouHaveQuestionsLeft;
	public float AnswerDeltaTime => Time.unscaledTime - _currentAnswerStartTime;
	public bool YourScoreIsHigher => _yourScore > _otherScore;
	public bool OtherScoreIsHigher => _otherScore > _yourScore;
	public bool YouHaveQuestionsLeft => _yourQuestionsLeft > 0;
	public bool OtherHaveQuestionsLeft => _otherQuestionsLeft > 0;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this);
		}
	}

	private void Reset()
	{
		_currentAnswerStartTime = 0f;
		_yourScore = 0;
		_yourQuestionsLeft = 0;
		_otherName = string.Empty;
		_otherQuestionsLeft = 0;
		_otherScore = 0;
	}

	public IEnumerator RunGame()
	{
		yield return StartCoroutine(GameStart());
		yield return StartCoroutine(GameLoop());
		GameEnd();
	}

	private IEnumerator GameStart()
	{
		Reset();
		yield return StartCoroutine(GetMatchStatus());
		yield return StartCoroutine(LoadQuestion());
	}

	private IEnumerator GameLoop()
	{
		while (GameRunning)
		{
			yield return StartCoroutine(GetMatchStatus());
			UpdateUI();
			yield return new WaitForSeconds(1);
			if (WaitingForEnd)
				OnYourPlayerFinish();
		}
	}

	private void GameEnd()
	{
		OnBothPlayersFinish();
	}

	private IEnumerator GetMatchStatus()
	{
		yield return StartCoroutine(APIManager.Instance.GetMatchStatus(APIManager.Instance.UpdateMatchStatus));
	}

	public void OnGetMatchStatusSuccess(int yourScore, int otherScore, int yourQuestionsLeft, int otherQuestionsLeft, string otherName)
	{
		_yourScore = yourScore;
		_otherScore = otherScore;
		_yourQuestionsLeft = yourQuestionsLeft;
		_otherQuestionsLeft = otherQuestionsLeft;
		_otherName = otherName;
	}

	private void UpdateUI()
	{
		_inGameMenu.UpdatePlayerStats(_yourScore, _otherScore, _yourQuestionsLeft, _otherQuestionsLeft);
	}

	private void OnYourPlayerFinish()
	{
		if (WaitingForEnd)
		{
			_inGameMenu.ShowEndScreen();
			_inGameMenu.ShowEndMessage($"Waiting for {_otherName} to finish");
		}
	}

	private void OnBothPlayersFinish()
	{
		_inGameMenu.ShowEndScreen();
		_inGameMenu.ShowEndMessage(GetResultsText());
		_inGameMenu.EnableExitButton();
	}

	private string GetResultsText()
	{
		string message;
		if (YourScoreIsHigher)
			message = "Congratulations!\nYou have won the match";
		else if (OtherScoreIsHigher)
			message = $"Womp Womp...\nIt seems that {_otherName} has defeated you";
		else
			message = "WOAH!\nIt seems that this match was a tie!";
		return message;
	}

	public void UpdatePlayerName(string name)
	{
		_yourName = name;
	}

	public IEnumerator LoadQuestion()
	{
		if (YouHaveQuestionsLeft)
			yield return StartCoroutine(APIManager.Instance.GetNextQuestion());
	}

	public void StartTimer()
	{
		_currentAnswerStartTime = Time.unscaledTime;
	}
}
