using UnityEngine;
using TMPro;
using DG.Tweening;

namespace UI
{
	public class Bar : MonoBehaviour
	{
		[SerializeField] private RectTransform _barTransform;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private bool _vertical;

		public void Reset()
		{
			_text.text = string.Empty;
			_barTransform.localScale = Vector3.one;
		}

		public void UpdateText(string text)
		{
			_text.text = text;
		}

		public void UpdateBar(float value, bool tween = false)
		{
			Debug.Assert(value <= 1f & value >= 0f);
			Vector3 newScale = _vertical ? new(1, value, 1) : new(value, 1, 1);
			if (tween)
				_barTransform.DOScale(newScale, 0.5f).SetEase(Ease.OutCubic);
			else
				_barTransform.localScale = newScale;
		}
	}
}
