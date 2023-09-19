using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;

    const string API_URL = "https://localhost:7166/api/";

    public IEnumerator JoinGame(string PlayerName)
    {
        WWWForm form = new WWWForm();
        form.AddField("PlayerName", PlayerName);

        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "ConnectToServer", form))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    uiManager.ConnectToServerSuccess();
                    break;
            }
        }
    }

    public IEnumerator TryStartGame()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound"))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    uiManager.TriggerWaitingText();
                    break;

            }
        }
    }


    /*public void GetQuestionText(string id) 
    {
        StartCoroutine(GetQuestionTextCor(id));
    }

    IEnumerator GetQuestionTextCor(string id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Question/" + id))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    uiManager.UpdateQuestionText(request.downloadHandler.text);
                    break;
            }
        }
    }

    IEnumerator GetQuestion(int id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Questions/"+id))
        {
            yield return request.SendWebRequest();
            switch(request.result) 
            { 
                case UnityWebRequest.Result.Success:
                    Debug.Log(request.downloadHandler.text);
                    
                    break;
            }
        }
    }*/
}
