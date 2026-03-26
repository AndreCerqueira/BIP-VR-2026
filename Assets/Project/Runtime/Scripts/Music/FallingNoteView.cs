using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class FallingNoteView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Vector3 _targetOffset;
        
        [Header("Visual Settings")]
        [SerializeField, Range(0f, 1f)] private float _defaultAlpha = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _hitAlpha = 0.4f;
        
        private Transform _targetKey;
        private Material _materialInstance;
        private float _hitTime;
        private string _colorProperty = "_Color";
        
        private const float FADE_DURATION = 0.1f;

        private void Awake()
        {
            if (_renderer == null) return;
            
            _materialInstance = _renderer.material;
            
            if (_materialInstance.HasProperty("_BaseColor"))
                _colorProperty = "_BaseColor";
        }

        public void Initialize(Transform targetKey, float hitTime, float length, Color color)
        {
            if (targetKey == null) return;

            _targetKey = targetKey;
            _hitTime = hitTime;
            
            var scale = transform.localScale;
            scale.y = length;
            transform.localScale = scale;
            
            if (_materialInstance != null)
            {
                color.a = _defaultAlpha;
                _materialInstance.SetColor(_colorProperty, color);
            }
        }

        public void UpdatePosition(float currentSongTime, float fallSpeed)
        {
            if (_targetKey == null) return;

            var timeDifference = _hitTime - currentSongTime;
            
            var targetPos = _targetKey.position + _targetOffset;
            targetPos.y += (timeDifference * fallSpeed) + (transform.localScale.y / 2f);
            
            transform.position = targetPos;
        }

        public void HandleHit()
        {
            if (_materialInstance == null) return;
            
            var currentColor = _materialInstance.GetColor(_colorProperty);
            currentColor.a = _hitAlpha;
            
            _materialInstance.DOColor(currentColor, _colorProperty, FADE_DURATION);
        }

        public void HandleMiss()
        {
            if (_materialInstance == null) return;
            
            var missColor = Color.red;
            missColor.a = _defaultAlpha;
            
            _materialInstance.DOColor(missColor, _colorProperty, FADE_DURATION);
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                _materialInstance.DOKill();
                Destroy(_materialInstance);
            }
        }
    }
}