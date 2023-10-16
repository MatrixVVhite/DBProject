using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] TMP_Text answer;
    [SerializeField] UIManager manager;
    [SerializeField] Image button;
    [SerializeField] Color defaultColor;
    private string AnswerID;
    bool flag;
    

    public void updateAnswer(string answerID, string Text)
    {
        button.color = defaultColor;
        AnswerID = answerID;
        answer.text = Text;
    }


    public void SubmitAnswer()
    {
        flag = manager.SubmitAnswer(AnswerID);
        defaultColor = button.color;
        if (flag)  button.color = Color.green;
        else button.color = Color.red;
    }
}
