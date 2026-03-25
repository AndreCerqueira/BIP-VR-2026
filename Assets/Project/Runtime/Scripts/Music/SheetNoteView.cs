using DG.Tweening;
using Project.Runtime.Scripts.Music.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Music
{
    public class SheetNoteView : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private Image _image;

        [Header("Animation Settings")]
        [SerializeField] private float _pulseDuration = 0.5f;      
        [SerializeField] private float _minPulseScale = 0.8f;
        [SerializeField] private float _maxPulseScale = 1.2f;

        private SheetNote _data;
        private Vector2 _baseSize;
        private TMP_Text _label;
        private Sequence _idleSequence;

        private const float COLOR_TWEEN_DURATION = 0.15f;

        public int MidiNote => _data?.MidiNote ?? -1;
        public bool IsRest => _data?.IsRest ?? true;
        public float Duration => _data?.Duration ?? 0f;

        private void Awake()
        {
            if (_image == null) return;
            
            _baseSize = _image.rectTransform.sizeDelta;
        }

        public void Initialize(SheetNote data, TMP_Text label, float widthMultiplier = 1f, float heightMultiplier = 1f)
        {
            _data = data;
            _label = label;
            
            UpdateSprites();
            
            var scaledSize = new Vector2(_baseSize.x * widthMultiplier, _baseSize.y * heightMultiplier);
            
            if (_image != null)
                _image.rectTransform.sizeDelta = scaledSize;
        }

        public void StartIdleAnimation()
        {
            if (IsRest || _image == null) return;

            StopIdleAnimation();

            _idleSequence = DOTween.Sequence();

            _image.transform.localScale = Vector3.one * _minPulseScale;

            var scaleTween = _image.transform.DOScale(Vector3.one * _maxPulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            _idleSequence.Append(scaleTween);
        }

        public void StopIdleAnimation()
        {
            if (_idleSequence != null && _idleSequence.IsPlaying())
                _idleSequence.Kill();

            _image.transform.localScale = Vector3.one;
        }

        public void SetColor(Color targetColor)
        {
            _image?.DOKill(true);
            _label?.DOKill(true);

            if (_image != null)
                _image.DOColor(targetColor, COLOR_TWEEN_DURATION);
                
            if (_label != null)
                _label.DOColor(targetColor, COLOR_TWEEN_DURATION);
        }

        private void UpdateSprites()
        {
            if (NotationSpriteProvider.Instance == null) return;
            
            var sprite = NotationSpriteProvider.Instance.GetSprite(_data.Duration, _data.IsRest);
            
            if (_image != null)
                _image.sprite = sprite;
        }
    }
}