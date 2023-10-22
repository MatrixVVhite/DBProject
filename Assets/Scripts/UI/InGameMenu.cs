using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
	public class InGameMenu : MonoBehaviour
	{
		#region CORE_OBJECTS
		[SerializeField] private GameManager _gameManager;
		[SerializeField] private APIManager _APIManager;
		[SerializeField] private UIManager _UIManager;
		#endregion

		#region FIELDS
		[SerializeField] TextMeshProUGUI _question;
		[SerializeField] AnswerButton[] _answers;
		[SerializeField] TextMeshProUGUI[] _playerStats;
		[SerializeField] GameObject _endGameScreen;
		[SerializeField] TextMeshProUGUI _finalMessage;
		[SerializeField] Button _exitMatchEnd;
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
					_answers[i].UpdateAnswer((i + 1).ToString(), _currentQuestion["Answer" + (i + 1)]);
			}
			catch (KeyNotFoundException)
			{
				return;
			}
		}

		public IEnumerator SubmitAnswer(string AnswerID, AnswerButton button)
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
			_UIManager.ExitMatch();
		}
		#endregion
	}
}
