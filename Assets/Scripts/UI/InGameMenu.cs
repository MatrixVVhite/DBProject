using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
	public class InGameMenu : MonoBehaviour
	{
		#region FIELDS
		[SerializeField] private TextMeshProUGUI _question;
		[SerializeField] private AnswerButton[] _answers;
		[SerializeField] private PlayerStatusSidebar _playerStats;
		[SerializeField] private PlayerStatusSidebar _otherStats;
		[SerializeField] private Bar _timerBar;
		[SerializeField] private GameObject _endGameScreen;
		[SerializeField] private TextMeshProUGUI _finalMessage;
		[SerializeField] private Button _exitMatchButton;
		private Dictionary<string, string> _currentQuestion;
		private Coroutine _animateTimerCoroutine;
		#endregion

		#region FUNCTIONS
		public void Reset()
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
				ShuffleAnswers();
				for (int i = 0; i < 4; i++)
					_answers[i].UpdateAnswer(i + 1, _currentQuestion["Answer" + (i + 1)]);
			}
			catch (KeyNotFoundException)
			{
				return;
			}
		}

		public IEnumerator SubmitAnswer(int answerID, float answerTime, AnswerButton button)
		{
			DisableAllButtons();
			yield return StartCoroutine(APIManager.Instance.AnswerQuestion(
				answerID,
				answerTime,
				button.ColorResponse,
				UpdatePlayerScore
			));
			yield return new WaitForSeconds(1);
			StartCoroutine(GameManager.Instance.LoadQuestion());
		}

		public void UpdateYourName(string name)
		{
			_playerStats.SetName(name);
		}

		public void UpdateOtherName(string name)
		{
			_otherStats.SetName(name);
		}

		public void UpdatePlayerScore(int yourScore)
		{
			_playerStats.UpdateScore(yourScore);
		}

		public void UpdatePlayerStats(int yourScore, int otherScore, int yourQuestionsLeft, int otherQuestionsLeft)
		{
			UpdatePlayerScore(yourScore);
			_playerStats.UpdateQuestions(yourQuestionsLeft);
			_otherStats.UpdateScore(otherScore);
			_otherStats.UpdateQuestions(otherQuestionsLeft);
		}

		public void ShowEndScreen()
		{
			_endGameScreen.SetActive(true);
		}

		public void ShowEndMessage(string message)
		{
			_finalMessage.text = message;
		}

		public void EnableExitButton()
		{
			_exitMatchButton.interactable = true;
		}

		public void OnExitMatchButtonClicked()
		{
			StartCoroutine(APIManager.Instance.LeaveMatch());
		}

		public void OnExitMatchSuccessful()
		{
			UIManager.Instance.ExitMatch();
		}

		private IEnumerator AnimateTimerCoroutine()
		{
			while(GameManager.Instance.AnswerTimeLeft > 0)
			{
				_timerBar.UpdateBar(GameManager.Instance.AnswerTimeLeft/GameManager.ANSWER_TIME_LIMIT);
				_timerBar.UpdateText(Mathf.Round(GameManager.Instance.AnswerTimeLeft).ToString());
				yield return null;
			}
		}

		public void AnimateTimer()
		{
			StopAnimateTimer();
			_animateTimerCoroutine = StartCoroutine(AnimateTimerCoroutine());
		}

		public void StopAnimateTimer()
		{
			if (_animateTimerCoroutine is not null)
				StopCoroutine(_animateTimerCoroutine);
		}

		public void DisableAllButtons()
		{
			foreach (var answerButton in _answers)
				answerButton.DisableButton();
		}

		private void ShuffleAnswers()
		{
			int n = _answers.Length;
			while (n > 1)
			{
				int k = Random.Range(0, n--);
				(_answers[k], _answers[n]) = (_answers[n], _answers[k]);
			}
		}
		#endregion
	}
}
