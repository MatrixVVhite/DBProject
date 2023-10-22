using UnityEngine;
using TMPro;

namespace UI
{
	public class PlayerStatusSidebar : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _name;
		[SerializeField] private Bar _scoreBar;
		[SerializeField] private Bar _questionsLeft;

		public void Reset()
		{
			SetName(string.Empty);
			_scoreBar.Reset();
			_questionsLeft.Reset();
		}

		public void SetName(string name)
		{
			_name.text = name;
		}

		public void UpdateScore(int score)
		{
			_scoreBar.UpdateText($"Score: {score}");
			_scoreBar.UpdateBar(score/(float)GameManager.MAX_SCORE_PER_MATCH);
		}

		public void UpdateQuestions(int questionsLeft)
		{
			if (questionsLeft > 0)
			{
				int questionNumber = GameManager.QUESTIONS_PER_MATCH - questionsLeft;
				_questionsLeft.UpdateText($"Question #{questionNumber + 1}");
				_questionsLeft.UpdateBar(questionNumber / (float)GameManager.QUESTIONS_PER_MATCH);
			}
			else
			{
				_questionsLeft.UpdateText($"Done!");
				_questionsLeft.UpdateBar(1f);
			}
		}
	}
}
