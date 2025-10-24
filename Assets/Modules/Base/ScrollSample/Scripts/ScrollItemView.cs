using TMPro;
using UnityEngine;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// View component for individual scroll list item
    /// </summary>
    public class ScrollItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text indexText;
        [SerializeField] private RectTransform rectTransform;

        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(int index)
        {
            if (indexText != null)
                indexText.text = index.ToString();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}

