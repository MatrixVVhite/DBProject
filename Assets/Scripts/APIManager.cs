using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UI;

public class APIManager : MonoBehaviour
{
	const string API_URL = "https://localhost:7166/api/";

	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private InGameMenu _inGameMenu;
	private int _token = 0;
	private int _matchID = 0;
	private bool _queueFlag = true;
	private bool _ticketValid = true;

	public static APIManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this);
		}
	}

	public IEnumerator ConnectToServer(string playerName)
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerName", playerName)
		};
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "ConnectToServer", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				GameManager.Instance.UpdateYourName(playerName);
				_token = int.Parse(request.downloadHandler.text);
				_mainMenu.OnConnectToServerSuccess();
				break;
		}
	}

	public IEnumerator DisconnectFromServer()
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerToken", _token.ToString())
		};
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "DisconnectFromServer", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				_mainMenu.OnDisconnectFromServerSuccess();
				break;
		}
	}

	public IEnumerator SubmitTicket()
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerToken", _token.ToString())
		};
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "SubmitTicket", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				_mainMenu.OnJoinQueueSuccess();
				StartCoroutine(TryLoadMatch());
				break;
		}
	}

	public IEnumerator IsTicketValid(System.Action<bool> callback)
	{
		using UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsTicketValid/" + _token);
		yield return request.SendWebRequest();
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

	public IEnumerator RevokeTicket()
	{
		yield return StartCoroutine(IsTicketValid((callback) => { _ticketValid = callback; }));
		if (_ticketValid && _queueFlag)
		{
			List<IMultipartFormSection> formData = new()
			{
				new MultipartFormDataSection("playerToken", _token.ToString())
			};
			using UnityWebRequest request = UnityWebRequest.Post(API_URL + "RevokeTicket", formData);
			yield return request.SendWebRequest();
			switch (request.result)
			{
				case UnityWebRequest.Result.Success:
					_queueFlag = false;
					_mainMenu.OnLeftQueueSuccess();
					break;
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
			using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchFound/" + _token))
			{
				yield return request.SendWebRequest();
				switch (request.result)
				{
					case UnityWebRequest.Result.Success:
						var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
						bool _matchFound = bool.Parse(json["Found"]);
						if (_matchFound)
						{
							_matchID = int.Parse(json["MatchID"]);
							_mainMenu.OnMatchFound(json["OtherPlayerName"]);
							_queueFlag = false;
						}
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
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "JoinMatch", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				_queueFlag = false;
				string joinBool = "false";
				while (joinBool == "false")
				{
					yield return IsMatchActive((callback) => { joinBool = callback; });
				}
				yield return StartCoroutine(GetMatchStatus(UpdateMatchStatus));
				StartCoroutine(GameManager.Instance.RunGame());
				_mainMenu.OnStartGame();
				break;
		}
	}

	public void UpdateMatchStatus(Dictionary<string, string> statusDict)
	{
		GameManager.Instance.OnGetMatchStatusSuccess(
			int.Parse(statusDict["YourScore"]),
			int.Parse(statusDict["OtherScore"]),
			int.Parse(statusDict["YourQuestionsLeft"]),
			int.Parse(statusDict["OtherQuestionsLeft"]),
			statusDict["OtherPlayerName"]);
	}

	public IEnumerator LeaveMatch()
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerToken", _token.ToString())
		};
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "LeaveMatch", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				_inGameMenu.OnExitMatchSuccessful();
				break;
		}
	}

	public IEnumerator IsMatchActive(System.Action<string> StatusCallback)
	{
		using UnityWebRequest request = UnityWebRequest.Get(API_URL + "IsMatchActive/" + _token);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				StatusCallback(request.downloadHandler.text);
				break;
		}
	}

	public IEnumerator GetMatchStatus(System.Action<Dictionary<string, string>> statusCallback)
	{
		using UnityWebRequest request = UnityWebRequest.Get(API_URL + "GetMatchStatus?matchID=" + _matchID + "&playerToken=" + _token);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				statusCallback(JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text));
				break;
		}
	}

	public IEnumerator GetNextQuestion()
	{
		using UnityWebRequest request = UnityWebRequest.Get(API_URL + "GetNextQuestion/" + _token);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				GameManager.Instance.StartTimer();
				_inGameMenu.UpdateQuestion(JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text));
				_inGameMenu.UpdateQuestionUI();
				break;
		}
	}

	public IEnumerator AnswerQuestion(int answerID, float answerTime, System.Action<bool> answerResult = null, System.Action<int> updateScore = null)
	{
		List<IMultipartFormSection> formData = new()
		{
			new MultipartFormDataSection("playerToken", _token.ToString()),
			new MultipartFormDataSection("answerID", answerID.ToString()),
			new MultipartFormDataSection("answerTime", answerTime.ToString())
		};
		using UnityWebRequest request = UnityWebRequest.Post(API_URL + "AnswerQuestion", formData);
		yield return request.SendWebRequest();
		switch (request.result)
		{
			case UnityWebRequest.Result.Success:
				var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
				if (!IsNullOrEmpty(result))
				{
					if (answerResult is not null)
						answerResult(bool.Parse(result["Correct"]));
					if (updateScore is not null)
						updateScore(int.Parse(result["Score"]));
				}
				break;
		}
	}

	private static bool IsNullOrEmpty(Dictionary<string, string> dict)
	{
		return dict is null || dict.Count == 0;
	}
}
