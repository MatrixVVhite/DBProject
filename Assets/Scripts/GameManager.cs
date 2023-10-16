using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] APIManager apiManager;
    private string player;
    private bool GameRunning = false;
    private int questionsLeft = 0;
    private Dictionary<string, string> MatchStats;

    public void RunGame()
    {
        RefreshStats();
        GameRunning = true;
        questionsLeft = int.Parse(MatchStats[player + "QuestionsLeft"]);
        LoadQuestion();
        while (GameRunning)
        {
            RefreshStats();




            new WaitForSeconds(1);
            if (questionsLeft <= 0) GameRunning = false;
        }
    }


    public void LoadQuestion()
    {
        if (questionsLeft > 0)
        {
            apiManager.GetNextQuestion();
        }
    }

    private void RefreshStats()
    {
        StartCoroutine(apiManager.GetMatchStatus((Status) => { MatchStats = Status; }));
    }
}
