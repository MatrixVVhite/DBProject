using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class APIManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] bool TestFlag = false;
    private int _token = 0;
    private string _MatchID = "0";
    private bool _queueflag = true;

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
                    _token = int.Parse(request.downloadHandler.text);
                    StartCoroutine( JoinQueue());
                    break;
            }
        }
    }

    public IEnumerator JoinQueue()
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormDataSection("playerToken", _token.ToString())
        };
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "SubmitTicket", formData))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    uiManager.ConnectToServerSuccess();
                    StartCoroutine(TryLoadMatch());
                    break;
            }
        }
    }

    public IEnumerator IsTicketValid(System.Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsTicketValid/" + _token))
        {
            yield return request.SendWebRequest();
            Debug.Log(request.result);
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    if (request.downloadHandler.text == "1") callback(true);
                    else callback(false);
                    break;
                default: callback(false); break;



            }
        }
    }

    public bool IsTicketValidOutput()
    {
        bool output = false;
        StartCoroutine(IsTicketValid((_isTicketValid) =>
        {
            output = _isTicketValid;

        }));
        return output;
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

    public IEnumerator LeaveQueue()
    {
        if (IsTicketValidOutput())
        {
            List<IMultipartFormSection> formData = new()
            {
                new MultipartFormDataSection("playerToken", _token.ToString())
            };
            using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "RevokeTicket", formData))
            {
                yield return request.SendWebRequest();
                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        Debug.Log("Left Queue");
                        _queueflag = false;
                        StartCoroutine(LeaveGame());
                        break;
                }
            }
        }
        
    }

    public IEnumerator TryLoadMatch()
    {
        _queueflag = true;
        while (IsTicketValidOutput() && _queueflag)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound/"+_token))
            {
                yield return request.SendWebRequest();
                Debug.Log(request.result);
                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        if (request.downloadHandler.text == "0" && TestFlag == false) 
                        {
                            uiManager.TriggerWaitingText();
                            yield return new WaitForSecondsRealtime(1);
                            Debug.Log("Match Not Found");
                            break;
                        }
                        _queueflag = false;
                        _MatchID = request.downloadHandler.text;
                        uiManager.MatchFound();
                        Debug.Log("result success");
                        break;
               
                    

                }
            }
        }
    }

    public IEnumerator JoinMatch()
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormDataSection("playerToken", _token.ToString())
        };
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "JoinMatch", formData))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("Disconnect Attempt Post Successful");
                    uiManager.StartGame();
                    break;
            }
        }
    }

    public IEnumerator AbandonMatch()
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormDataSection("playerToken", _token.ToString())
        };
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "AbandonMatch", formData))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log("Disconnect Attempt Post Successful");
                    break;
            }
        }
    }

    public IEnumerator GetMatchStatus(System.Action<Dictionary<string,string>> StatusCallback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "GetMatchStatus?matchID=" + _MatchID + "&playerToken=" + _token))
        {
            yield return request.SendWebRequest();
            Debug.Log(request.result);
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    StatusCallback(JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text));
                    break;
            }
        }
    }

    public IEnumerator GetNextQuestion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "GetNextQuestion/" + _token))
        {
            yield return request.SendWebRequest();
            Debug.Log(request.result);
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    uiManager.UpdateQuestion(JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text));
                    uiManager.UpdateQuestionUI();
                    break;
            }
        }
    }

    public IEnumerator AnswerQuestion(string answerID, System.Action<bool> answerResult)
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormDataSection("playerToken", _token.ToString()),
            new MultipartFormDataSection("answerID", answerID)
        };
        using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "AnswerQuestion", formData))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    answerResult(request.downloadHandler.text == "true");
                    break;
            }
        }
    }
}
