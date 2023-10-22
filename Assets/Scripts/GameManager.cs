using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

public class GameManager : MonoBehaviour
{
	public const int QUESTIONS_PER_MATCH = 10;
	public const int MAX_SCORE_PER_QUESTION = 10;
	public const int MAX_SCORE_PER_MATCH = QUESTIONS_PER_MATCH * MAX_SCORE_PER_QUESTION;
	public const int GAME_FINISHED_QUESTION_COUNT = QUESTIONS_PER_MATCH + 1;
	[SerializeField] private InGameMenu _inGameMenu;
	private string _player;
	private string _otherPlayer;
	private bool _disabled = false;
	private bool _gameRunning = false;
	private bool _waitingForEnd = false;
	private int _questionsLeft = 0;
	private float _currentAnswerStartTime = 0f;
	private Dictionary<string, string> _matchStats;

	public static GameManager Instance { get; private set; }

	public float AnswerDeltaTime { get => Time.unscaledTime - _currentAnswerStartTime; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this);
		}
	}

	public IEnumerator RunGame(Dictionary<string, string> newMatchStats) // TODO Clean this unsightly mess
	{
		_matchStats = newMatchStats;
		_gameRunning = true;
		_player = "P" + _matchStats["YouAre"];
		_otherPlayer = "P" + (1 + (int.Parse(_matchStats["YouAre"])%2));
		_questionsLeft = int.Parse(_matchStats[_player + "QuestionsLeft"]);
		LoadQuestion();
		while (_gameRunning)
		{
			if (!_disabled)
			{
				_inGameMenu.UpdatePlayerStats(int.Parse(_matchStats[_player + "Score"]), int.Parse(_matchStats[_otherPlayer + "Score"]), int.Parse(_matchStats[_player + "QuestionsLeft"]), int.Parse(_matchStats[_otherPlayer + "QuestionsLeft"]));
				_questionsLeft = int.Parse(_matchStats[_player + "QuestionsLeft"]);
				yield return StartCoroutine(APIManager.Instance.GetMatchStatus((Status) => { _matchStats = Status; }));
				yield return StartCoroutine(Cooldown(1));
				if (_questionsLeft <= 0)
					_gameRunning = false;
			}
		}
		_waitingForEnd = true;
		_inGameMenu.LoadEndScreen();
		while (_waitingForEnd)
		{
			if (!_disabled)
			{
				_inGameMenu.UpdatePlayerStats(int.Parse(_matchStats[_player + "Score"]), int.Parse(_matchStats[_otherPlayer + "Score"]), int.Parse(_matchStats[_player + "QuestionsLeft"]), int.Parse(_matchStats[_otherPlayer + "QuestionsLeft"]));
				_questionsLeft = int.Parse(_matchStats[_otherPlayer + "QuestionsLeft"]);
				yield return StartCoroutine(APIManager.Instance.GetMatchStatus((Status) => { _matchStats = Status; }));
				yield return StartCoroutine(Cooldown(1));
				if (_questionsLeft <= 0)
					_waitingForEnd = false;
			}
		}
		string message = "";
		if (int.Parse(_matchStats[_player+"Score"])> int.Parse(_matchStats[_otherPlayer + "Score"]))
			message = "Congratulations!\n" + "You have won the match";
		else if (int.Parse(_matchStats[_player + "Score"]) < int.Parse(_matchStats[_otherPlayer + "Score"]))
			message = "Womp Womp...\n" + "It seems that your opponent has defeated you";
		else
			message = "WOAH!\n" + "It seems that this match was a tie!";
		_inGameMenu.UpdateEndMessage(message);
	}

	public void LoadQuestion()
	{
		if (_gameRunning)
			StartCoroutine(APIManager.Instance.GetNextQuestion());
	}

	public void StartTimer()
	{
		_currentAnswerStartTime = Time.unscaledTime;
	}

	private void RefreshStats()
	{
		StartCoroutine(APIManager.Instance.GetMatchStatus((Status) => { _matchStats = Status; }));
	}

	private IEnumerator Cooldown(int cooldown)
	{
		_disabled = true;
		yield return new WaitForSeconds(cooldown);
		_disabled = false;
	}
}
