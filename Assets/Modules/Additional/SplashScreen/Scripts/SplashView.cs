using CodeBase.Core.UI.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Additional.SplashScreen.Scripts
{
    public class SplashView : BaseView
    {
        [Header("Progress UI Components")]
        [SerializeField] private CanvasGroup progressBarCanvasGroup;
        [SerializeField] private TMP_Text progressValueText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressBar;
        
        private CompositeDisposable _disposables = new();

        public void SetupEventListeners(ReadOnlyReactiveProperty<string> progressStatus,
            ReadOnlyReactiveProperty<float> exponentialProgress)
        { 
            Observable.CombineLatest(exponentialProgress, progressStatus,
                    (progress, status) => new { progress, status })
                .Subscribe(data => ReportProgress(data.progress, data.status).Forget())
                .AddTo(_disposables);
        }
        private UniTask ReportProgress(float expProgress, string progressStatus)
        {
            progressText.text = progressStatus;
    
            return DOTween.To(() => progressBar.fillAmount, x =>
            {
                progressBar.fillAmount = x;
                progressValueText.text = $"{(int)(x * 100)}%";
            }, expProgress, 1f).ToUniTask();
        }
        
        public override UniTask Show()
        {
            base.Show().Forget();
            SetActive(true);
            progressBar.fillAmount = 0;
            return UniTask.CompletedTask;
        }
    }
}