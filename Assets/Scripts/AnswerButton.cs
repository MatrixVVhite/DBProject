using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerButton : MonoBehaviour
{
	[SerializeField] private UIManager _uiManager;
	[SerializeField] private Button _button;
	[SerializeField] private Image _buttonVisual;
	[SerializeField] private TMP_Text _answerText;
    [SerializeField] private Color _defaultColor;
    private string AnswerID;

#if UNITY_EDITOR
	private void OnValidate()
	{
		_button = GetComponent<Button>();
		_buttonVisual = GetComponent<Image>();
		_answerText = GetComponentInChildren<TMP_Text>();
	}
#endif

	public void UpdateAnswer(string answerID, string Text)
    {
        _buttonVisual.color = _defaultColor;
        AnswerID = answerID;
        _answerText.text = Text;
        _button.interactable = true;
    }

    public void SubmitAnswer()
    {
        StartCoroutine(_uiManager.SubmitAnswer(AnswerID, this));
        _button.interactable = false;
    }

    public void ColorResponse(bool flag)
    {
        _defaultColor = _buttonVisual.color;
		_buttonVisual.color = flag ? Color.green : Color.red;
	}
}
