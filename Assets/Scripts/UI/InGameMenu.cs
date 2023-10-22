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
		[SerializeField] private Bar _timerBar; // TODO Animate this
		[SerializeField] private GameObject _endGameScreen;
		[SerializeField] private TextMeshProUGUI _finalMessage;
		[SerializeField] private Button _exitMatchButton;
		private Dictionary<string, string> _currentQuestion;
		#endregion

		#region FUNCTIONS
		public void ResetUI()
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
					_answers[i].UpdateAnswer(i + 1, _currentQuestion["Answer" + (i + 1)]);
			}
			catch (KeyNotFoundException)
			{
				return;
			}
		}

		public IEnumerator SubmitAnswer(int AnswerID, float answerTime, AnswerButton button)
		{
			yield return StartCoroutine(APIManager.Instance.AnswerQuestion(
				AnswerID,
				answerTime,
				(isCorrect) => { button.ColorResponse(isCorrect); },
				(updatedScore) => { UpdatePlayerScore(updatedScore); }
			));
			yield return new WaitForSeconds(2);
			GameManager.Instance.LoadQuestion();
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

		public void LoadEndScreen()
		{
			_endGameScreen.SetActive(true);
		}

		public void UpdateEndMessage(string message)
		{
			_exitMatchButton.interactable = true;
			_finalMessage.text = message;
		}

		public void OnExitMatchButtonClicked()
		{
			StartCoroutine(APIManager.Instance.LeaveMatch());
		}

		public void OnExitMatchSuccessful()
		{
			UIManager.Instance.ExitMatch();
		}
		#endregion
	}
}
