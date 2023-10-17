using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] TMP_Text answer;
    [SerializeField] UIManager manager;
    [SerializeField] Image buttonVisual;
    [SerializeField] Button button;
    [SerializeField] Color defaultColor;
    private string AnswerID;
    

    public void updateAnswer(string answerID, string Text)
    {
        buttonVisual.color = defaultColor;
        AnswerID = answerID;
        answer.text = Text;
        button.interactable = true;
    }


    public void SubmitAnswer()
    {
        StartCoroutine(manager.SubmitAnswer(AnswerID, this));
        button.interactable = false;
    }

    public void ColorResponse(bool flag)
    {
        defaultColor = buttonVisual.color;
        if (flag) buttonVisual.color = Color.green;
        else buttonVisual.color = Color.red;
    }
}
