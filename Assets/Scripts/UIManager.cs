using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_InputField questionId;
    public TMP_Text questionText;
    public APIManager _APIManager;

    public void OnGetQuestionButtonClicked()
    {
        _APIManager.GetQuestionText(questionId.text);
    }

    public void UpdateQuestionText(string name)
    {
        questionText.text = name;
    }
}
