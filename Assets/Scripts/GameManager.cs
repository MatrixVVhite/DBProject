using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] APIManager apiManager;
    [SerializeField] int LengthOfGame = 5;
    private bool GameRunning = false;
    private int currentQuestion = 0;

    public void RunGame()
    {
        GameRunning = true;
        currentQuestion = 0;
        LoadQuestion();
        while (GameRunning)
        {
            




            new WaitForSeconds(1);
            if (currentQuestion >= LengthOfGame) GameRunning = false;
        }
    }


    public void LoadQuestion()
    {
        currentQuestion++;
        if (currentQuestion < LengthOfGame)
        {
            apiManager.GetNextQuestion();
        }
    }

    private void RefreshScore()
    {

    }
}
