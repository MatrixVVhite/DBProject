using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UI;

public class APIManager : MonoBehaviour
{
	const string API_URL = "https://localhost:7166/api/";

	[SerializeField] private GameManager _gameManager;
	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private InGameMenu _inGameMenu;
	private int _token = 0;
	private string _MatchID = "0";
	private bool _queueFlag = true;
	private bool _ticketValid = true;

	public IEnumerator ConnectToServer(string PlayerName)
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
					_mainMenu.OnConnectToServerSuccess();
					break;
			}
		}
	}

	public IEnumerator DisconnectFromServer()
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
					_mainMenu.OnDisconnectFromServerSuccess();
					break;
			}
		}
	}

	public IEnumerator SubmitTicket()
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
					_mainMenu.OnJoinQueueSuccess();
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
					if (request.downloadHandler.text == "true") callback(true);
					else callback(false);
					break;
				default: callback(false); break;
			}
			yield return new WaitForSecondsRealtime(1);
		}
	}

	public IEnumerator RevokeTicket()
	{
		yield return StartCoroutine(IsTicketValid((callback) => { _ticketValid = callback; }));
		if (_ticketValid && _queueFlag)
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
						_queueFlag = false;
						_mainMenu.OnLeftQueueSuccess();
						break;
				}
			}
		}
	}

	public IEnumerator TryLoadMatch()
	{
		_queueFlag = true;
		_ticketValid = true;
		yield return StartCoroutine(IsTicketValid((callback) => { _ticketValid = callback; }));
		while (_ticketValid & _queueFlag)
		{
			using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound/"+_token))
			{
				yield return request.SendWebRequest();
				Debug.Log(request.result);
				switch (request.result)
				{
					case UnityWebRequest.Result.Success:
						if (request.downloadHandler.text == "0") 
						{
							_mainMenu.TriggerWaitingText();
							Debug.Log("Match Not Found");
							break;
						}
						_MatchID = request.downloadHandler.text;
						_mainMenu.OnMatchFound();
						Debug.Log("result success");
						break;
				}
			}
			yield return StartCoroutine(IsTicketValid((callback) => { _ticketValid = callback; }));
			yield return new WaitForSeconds(1);
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
					Debug.Log("Match Join Post Successful");
					_queueFlag = false;
					string joinBool = "false";
					while (joinBool == "false")
					{
						yield return IsMatchActive((callback) => { joinBool = callback; });
					}
					yield return GetMatchStatus((newDict) => { StartCoroutine(_gameManager.RunGame(newDict)); }) ;
					_mainMenu.OnStartGame();
					break;
			}
		}
	}

	public IEnumerator LeaveMatch()
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerToken", _token.ToString())
		};
		using (UnityWebRequest request = UnityWebRequest.Post(API_URL + "LeaveMatch", formData))
		{
			yield return request.SendWebRequest();
			switch (request.result)
			{
				case UnityWebRequest.Result.Success:
					Debug.Log("Disconnect Attempt Post Successful");
					_inGameMenu.OnExitMatchSuccessful();
					break;
			}
		}
	}

	public IEnumerator IsMatchActive(System.Action<string> StatusCallback)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchActive/" + _token))
		{
			yield return request.SendWebRequest();
			Debug.Log(request.result);
			switch (request.result)
			{
				case UnityWebRequest.Result.Success:
					StatusCallback(request.downloadHandler.text);
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
					_inGameMenu.UpdateQuestion(JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text));
					_inGameMenu.UpdateQuestionUI();
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
