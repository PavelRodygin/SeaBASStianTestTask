using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Core.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        protected CanvasGroup CanvasGroup;
        protected Canvas Canvas;
        
        [field: SerializeField] public BaseAnimationElement AnimationElement { get; private set; }
        public bool IsActive { get; protected set; } = true;
        public bool IsInteractable { get; private set; }

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            Canvas = GetComponent<Canvas>();
        }

        public virtual async UniTask Show()
        {
            Debug.Log($"[BaseView] {gameObject.name} Show START");
            
            Debug.Log($"[BaseView] {gameObject.name} SetActive(true)");
            SetActive(true);
            
            if (IsActive && AnimationElement)
            {
                Debug.Log($"[BaseView] {gameObject.name} AnimationElement.Show START");
                await AnimationElement.Show();
                Debug.Log($"[BaseView] {gameObject.name} AnimationElement.Show COMPLETE");
            }
            
            IsInteractable = true;
            Debug.Log($"[BaseView] {gameObject.name} Show COMPLETE");
        }

        public virtual async UniTask Hide()
        {
            IsInteractable = false;

            if (IsActive && AnimationElement)
                await AnimationElement.Hide();
            SetActive(false);
        }
        
        protected void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;
            IsActive = isActive;
            if (Canvas) Canvas.enabled = isActive;

            if (CanvasGroup)
            {
                CanvasGroup.alpha = isActive ? 1 : 0;
                CanvasGroup.blocksRaycasts = isActive;
                CanvasGroup.interactable = isActive;
            }

            gameObject.SetActive(isActive);
        }
        
        public virtual void HideInstantly() => SetActive(false);
        
        public virtual void Dispose() { } 
    }
}