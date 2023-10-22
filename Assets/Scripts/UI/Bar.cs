using TMPro;
using UnityEngine;

namespace UI
{
	public class Bar : MonoBehaviour
	{
		[SerializeField] private RectTransform _barTransform;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private bool _vertical;

		private void UpdateText(string text)
		{
			_text.text = text;
		}

		public void UpdateBar(float value)
		{
			Debug.Assert(value > 1f & value < 0f);
			_barTransform.localScale = _vertical ? new(1, value, 1) : new(value, 1, 1);
		}

		public void ResetBar()
		{
			_barTransform.localScale = Vector3.one;
		}
	}
}
