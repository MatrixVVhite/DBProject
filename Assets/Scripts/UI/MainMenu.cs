using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
	public class MainMenu : MonoBehaviour
	{
		#region ELEMENTS
		[SerializeField] private GameObject _registration;
		[SerializeField] private TMP_InputField _playerNameInputField;
		[SerializeField] private Button _connectButton;
		[SerializeField] private Button _joinQueueButton;
		[SerializeField] private Button _startMatchButton;
		[SerializeField] private Button _exitGameButton;
		[SerializeField] private Button _disconnectButton;
		[SerializeField] private Button _leaveQueueButton;
		[SerializeField] private TextMeshProUGUI _menuText;
		#endregion

		#region FUNCTIONS
		#region BUTTONS
		public void OnConnectButtonClicked()
		{
			StartCoroutine(APIManager.Instance.ConnectToServer(_playerNameInputField.text));
		}

		public void OnJoinQueueButtonClicked()
		{
			StartCoroutine(APIManager.Instance.SubmitTicket());
		}

		public void OnStartMatchButtonClicked()
		{
			StartCoroutine(APIManager.Instance.JoinMatch());
			ShowMenuText("Waiting for other player...");
		}

		public void OnExitGameButtonClicked()
		{
			Application.Quit();
		}

		public void OnDisconnectButtonClicked()
		{
			StartCoroutine(APIManager.Instance.DisconnectFromServer());
		}

		public void OnLeaveQueueButtonClicked()
		{
			StartCoroutine(APIManager.Instance.RevokeTicket());
		}
		#endregion

		#region CALLBACKS
		public void OnConnectToServerSuccess()
		{
			_registration.SetActive(false);
			_connectButton.gameObject.SetActive(false);
			_exitGameButton.gameObject.SetActive(false);
			_joinQueueButton.gameObject.SetActive(true);
			_disconnectButton.gameObject.SetActive(true);
		}

		public void OnDisconnectFromServerSuccess()
		{
			_registration.SetActive(true);
			_connectButton.gameObject.SetActive(true);
			_exitGameButton.gameObject.SetActive(true);
			_joinQueueButton.gameObject.SetActive(false);
			_disconnectButton.gameObject.SetActive(false);
		}

		public void OnJoinQueueSuccess()
		{
			_joinQueueButton.gameObject.SetActive(true);
			_joinQueueButton.interactable = false;
			_leaveQueueButton.gameObject.SetActive(true);
			_disconnectButton.gameObject.SetActive(false);
			ShowMenuText("Looking for match...");
		}

		public void OnLeftQueueSuccess()
		{
			_joinQueueButton.gameObject.SetActive(true);
			_joinQueueButton.interactable = true;
			_leaveQueueButton.gameObject.SetActive(false);
			_disconnectButton.gameObject.SetActive(true);
			_connectButton.gameObject.SetActive(false);
			_startMatchButton.gameObject.SetActive(false);
			HideMenuText();
		}

		public void OnMatchFound(string otherName)
		{
			_joinQueueButton.gameObject.SetActive(false);
			_startMatchButton.gameObject.SetActive(true);
			ShowMenuText($"You are up against {otherName}!");
		}

		public void OnStartGame()
		{
			UIManager.Instance.ShowInGameMenu();
		}
		#endregion

		public void ShowMenuText(string text)
		{
			_menuText.text = text;
			_menuText.gameObject.SetActive(true);
		}

		public void HideMenuText()
		{
			_menuText.gameObject.SetActive(false);
		}
		#endregion
	}
}
