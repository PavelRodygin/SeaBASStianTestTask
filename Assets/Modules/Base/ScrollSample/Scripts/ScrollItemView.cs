using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.ScrollSampleModule.Scripts
{
    /// <summary>
    /// View component for individual scroll list item
    /// </summary>
    public class ScrollItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text indexText;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private LayoutElement layoutElement;

        public RectTransform RectTransform => rectTransform;
        public LayoutElement LayoutElement => layoutElement;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (layoutElement == null)
                layoutElement = GetComponent<LayoutElement>();
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

