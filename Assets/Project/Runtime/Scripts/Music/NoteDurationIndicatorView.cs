using DG.Tweening;
using Project.Runtime.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Music
{
    public class NoteDurationIndicatorView : Singleton<NoteDurationIndicatorView>
    {
        [Header("UI References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _durationText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.2f;
        
        [Header("White Key Offsets")]
        [SerializeField] private Vector3 _whiteKeyPositionOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 _whiteKeyRotationOffset = new Vector3(90f, 0f, 0f);

        [Header("Black Key Offsets")]
        [SerializeField] private Vector3 _blackKeyPositionOffset = new Vector3(0f, 1.2f, 0.5f);
        [SerializeField] private Vector3 _blackKeyRotationOffset = new Vector3(90f, 0f, 0f);

        private Tween _fillTween;
        private Tween _fadeTween;
        private Tween _textTween;

        public bool IsActive { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            if (_canvasGroup == null) return;
            if (_fillImage == null) return;
            
            _canvasGroup.alpha = 0f;
            _fillImage.fillAmount = 0f;
            IsActive = false;
        }

        public void Show(Transform targetTransform, float duration, bool isWhiteKey)
        {
            if (targetTransform == null) return;
            if (_fillImage == null) return;
            if (_canvasGroup == null) return;
            if (_durationText == null) return;

            IsActive = true;
            
            var positionOffset = isWhiteKey ? _whiteKeyPositionOffset : _blackKeyPositionOffset;
            var rotationOffset = isWhiteKey ? _whiteKeyRotationOffset : _blackKeyRotationOffset;
            
            transform.position = targetTransform.position + positionOffset;
            transform.rotation = targetTransform.rotation * Quaternion.Euler(rotationOffset);
            
            KillTweens();

            _fillImage.fillAmount = 0f;
            _canvasGroup.alpha = 0f;
            _durationText.text = "0.0s";

            _fadeTween = _canvasGroup.DOFade(1f, _fadeDuration);
            _fillTween = _fillImage.DOFillAmount(1f, duration).SetEase(Ease.Linear);
            
            _textTween = DOVirtual.Float(0f, duration, duration, UpdateTimeText).SetEase(Ease.Linear);
        }

        public void Hide()
        {
            if (!IsActive) return;
            if (_canvasGroup == null) return;

            IsActive = false;
            KillTweens();

            _fadeTween = _canvasGroup.DOFade(0f, _fadeDuration).OnComplete(ResetView);
        }

        private void UpdateTimeText(float timeValue)
        {
            if (_durationText != null)
            {
                var formattedTime = timeValue.ToString("F1");
                _durationText.text = $"{formattedTime}s";
            }
        }

        private void ResetView()
        {
            if (_fillImage == null) return;
            if (_durationText == null) return;
            
            _fillImage.fillAmount = 0f;
            _durationText.text = string.Empty;
        }

        private void KillTweens()
        {
            if (_fillTween != null && _fillTween.IsActive())
                _fillTween.Kill();
                
            if (_fadeTween != null && _fadeTween.IsActive())
                _fadeTween.Kill();
                
            if (_textTween != null && _textTween.IsActive())
                _textTween.Kill();
        }
        
        private void OnDestroy()
        {
            KillTweens();
        }
    }
}