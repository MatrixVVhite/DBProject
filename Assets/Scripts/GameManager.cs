using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private UIManager _UIManager;
	[SerializeField] private APIManager _APIManager;
	private string _player;
	private string _otherPlayer;
	private bool _disabled = false;
	private bool _gameRunning = false;
	private bool _waitingForEnd = false;
	private int _questionsLeft = 0;
	private Dictionary<string, string> _matchStats;

	public IEnumerator RunGame(Dictionary<string, string> newMatchStats)
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
				_UIManager.UpdateScores(_matchStats[_player + "Score"], _matchStats[_otherPlayer + "Score"], _matchStats[_player + "QuestionsLeft"], _matchStats[_otherPlayer + "QuestionsLeft"]);
				_questionsLeft = int.Parse(_matchStats[_player + "QuestionsLeft"]);
				yield return StartCoroutine(_APIManager.GetMatchStatus((Status) => { _matchStats = Status; }));
				yield return StartCoroutine(Cooldown(1));
				if (_questionsLeft <= 0)
					_gameRunning = false;
			}
		}

		_waitingForEnd = true;
		_UIManager.LoadEndScreen();
		while (_waitingForEnd)
		{
			if (!_disabled)
			{
				_UIManager.UpdateScores(_matchStats[_player + "Score"], _matchStats[_otherPlayer + "Score"], _matchStats[_player + "QuestionsLeft"], _matchStats[_otherPlayer + "QuestionsLeft"]);
				_questionsLeft = int.Parse(_matchStats[_otherPlayer + "QuestionsLeft"]);
				yield return StartCoroutine(_APIManager.GetMatchStatus((Status) => { _matchStats = Status; }));
				yield return StartCoroutine(Cooldown(1));
				if (_questionsLeft <= 0) _waitingForEnd = false;
			}
		}

		string message = "";

		if (int.Parse(_matchStats[_player+"Score"])> int.Parse(_matchStats[_otherPlayer + "Score"]))
		{
			message = "Congratulations!\n" + "you have won the match";   
		}
		else if (int.Parse(_matchStats[_player + "Score"]) < int.Parse(_matchStats[_otherPlayer + "Score"]))
		{
			message = "Womp Womp...\n" + "it seems that your opponent has defeated you";
		}
		else
		{
			message = "WOAH!\n" + "it seems that this match was a tie!";
		}
		_UIManager.UpdateEndMessage(message);
	}

	public void LoadQuestion()
	{
		if (_gameRunning)
			StartCoroutine(_APIManager.GetNextQuestion());
	}

	private void RefreshStats()
	{
		StartCoroutine(_APIManager.GetMatchStatus((Status) => { _matchStats = Status; }));
	}

	private IEnumerator Cooldown(int cooldown)
	{
		_disabled = true;
		yield return new WaitForSeconds(cooldown);
		_disabled = false;
	}
}
