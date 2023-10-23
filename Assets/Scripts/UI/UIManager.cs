using UnityEngine;

namespace UI
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private MainMenu _mainMenu;
		[SerializeField] private InGameMenu _inGameMenu;

		public static UIManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(this);
			}
		}

		public void ShowMainMenu()
		{
			_mainMenu.gameObject.SetActive(true);
			_inGameMenu.gameObject.SetActive(false);
		}

		public void ShowInGameMenu()
		{
			_mainMenu.gameObject.SetActive(false);
			_inGameMenu.gameObject.SetActive(true);
			_inGameMenu.Reset();
		}

		public void ExitMatch()
		{
			ShowMainMenu();
			_mainMenu.OnLeftQueueSuccess();
		}
	}
}
