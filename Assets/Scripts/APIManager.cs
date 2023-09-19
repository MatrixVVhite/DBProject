using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;

    const string API_URL = "https://localhost:7166/api/";

    public IEnumerator JoinGame(string PlayerName)
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerName", PlayerName)
		};
		using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "ConnectToServer", formData))
        {
            Debug.Log("Join Attempt Post Defined");
            yield return request.SendWebRequest();
            Debug.Log("Join Attempt Post Happened");
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("Join Attempt Post Successful");
                    uiManager.ConnectToServerSuccess();
                    break;
            }
        }
    }

    public IEnumerator TryStartGame()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound/"))
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
        StartCoroutine(GetQuestion(int.Parse(id)));
    }

    IEnumerator GetQuestionTextCor(string id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Question/" + id))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    //uiManager.UpdateQuestionText(request.downloadHandler.text);
                    break;
            }
        }
    }

    public IEnumerator GetQuestion(int id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Question/"+id))
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
