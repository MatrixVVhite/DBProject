using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    int _token;

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
                    _token = int.Parse(request.downloadHandler.text);
                    break;
            }
        }
    }

    public IEnumerator LeaveGame()
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormDataSection("playerToken", _token.ToString())
        };
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "DisconnectFromServer", formData))
        {
            Debug.Log("Disconnect Attempt Post Defined");
            yield return request.SendWebRequest();
            Debug.Log("Disconnect Attempt Post Happened");
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("Disconnect Attempt Post Successful");
                    break;
            }
        }
    }

    public IEnumerator TryStartGame()
    {
        Debug.Log("At least it tried");
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound/"+_token))
        {
            yield return request.SendWebRequest();
            Debug.Log(request.result);
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    if (request.downloadHandler.text == "0") {
                        uiManager.TriggerWaitingText();
                        Debug.Log("Match Not Found");
                        break;
                    }

                    
                    Debug.Log("result success");
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
