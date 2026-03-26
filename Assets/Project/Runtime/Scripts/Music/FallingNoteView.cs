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
        
        private Material _materialInstance;
        private float _targetY;
        private float _hitTime;
        
        private const float FADE_DURATION = 0.1f;

        private void Awake()
        {
            if (_renderer != null)
                _materialInstance = _renderer.material;
        }

        public void Initialize(Transform targetKey, float hitTime, float length, Color color, float targetY)
        {
            if (targetKey == null) return;

            _hitTime = hitTime;
            _targetY = targetY + (length / 2f) + _targetOffset.y;
            
            var startPos = targetKey.position + _targetOffset;
            transform.position = startPos;
            
            var scale = transform.localScale;
            scale.y = length;
            transform.localScale = scale;
            
            if (_materialInstance != null)
            {
                color.a = _defaultAlpha;
                _materialInstance.color = color;
            }
        }

        public void UpdatePosition(float currentSongTime, float fallSpeed)
        {
            var timeDifference = _hitTime - currentSongTime;
            var currentY = _targetY + (timeDifference * fallSpeed);
            
            var pos = transform.position;
            pos.y = currentY;
            transform.position = pos;
        }

        public void HandleHit()
        {
            if (_materialInstance != null)
                _materialInstance.DOFade(_hitAlpha, FADE_DURATION);
        }

        public void HandleMiss()
        {
            if (_materialInstance != null)
                _materialInstance.DOColor(Color.red, FADE_DURATION);
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