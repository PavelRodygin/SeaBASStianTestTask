using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.AnimationSampleModule.Scripts
{
    /// <summary>
    /// Component for animated UI square - animates position and rotation
    /// </summary>
    public class AnimatedSquareView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public Image Image { get; private set; }
        
        [Header("Animation Settings")]
        [SerializeField] private float movementRadius = 100f;
        [SerializeField] private float movementDuration = 2f;
        [SerializeField] private float rotationDuration = 3f;
        [SerializeField] private Ease movementEase = Ease.InOutSine;
        
        private Sequence _animationSequence;
        private Vector2 _initialPosition;

        private void Awake()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
            
            if (Image == null)
                Image = GetComponent<Image>();
        }

        public void Initialize(Vector2 position)
        {
            _initialPosition = position;
            RectTransform.anchoredPosition = position;
        }

        public async UniTask StartAnimation()
        {
            StopAnimation();

            _animationSequence = DOTween.Sequence();

            // Circular movement
            var randomAngle = Random.Range(0f, 360f);
            var targetPosition = _initialPosition + new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad) * movementRadius,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad) * movementRadius
            );

            // Animate position back and forth
            _animationSequence
                .Append(RectTransform.DOAnchorPos(targetPosition, movementDuration).SetEase(movementEase))
                .Append(RectTransform.DOAnchorPos(_initialPosition, movementDuration).SetEase(movementEase))
                .SetLoops(-1, LoopType.Restart);

            // Animate rotation continuously
            RectTransform.DORotate(new Vector3(0, 0, 360), rotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);

            _animationSequence.SetLink(gameObject);
            _animationSequence.Play();

            await UniTask.CompletedTask;
        }

        public void StopAnimation()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
            {
                _animationSequence.Kill();
                _animationSequence = null;
            }

            // Check if object is not destroyed before accessing
            if (this == null || RectTransform == null) return;

            RectTransform.DOKill();
            RectTransform.anchoredPosition = _initialPosition;
            RectTransform.rotation = Quaternion.identity;
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}

