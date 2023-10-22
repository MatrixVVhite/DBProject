using UnityEngine;

namespace UI
{
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private MainMenu _mainMenu;
		[SerializeField] private InGameMenu _inGameMenu;

		public void ShowMainMenu()
		{
			_mainMenu.gameObject.SetActive(true);
			_inGameMenu.gameObject.SetActive(false);
		}

		public void ShowInGameMenu()
		{
			_mainMenu.gameObject.SetActive(false);
			_inGameMenu.gameObject.SetActive(true);
			_inGameMenu.ResetUI();
		}

		public void ExitMatch()
		{
			ShowMainMenu();
			_mainMenu.OnLeftQueueSuccess();
		}
	}
}
