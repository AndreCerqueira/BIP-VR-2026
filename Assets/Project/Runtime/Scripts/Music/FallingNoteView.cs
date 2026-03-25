using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class FallingNoteView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Vector3 _targetOffset;
        
        private Tweener _fallTween;
        private Tweener _fadeTween;
        private Material _materialInstance;
        
        private const float FADE_DURATION = 0.2f;

        public float TargetY { get; private set; }

        private void Awake()
        {
            if (_renderer != null)
                _materialInstance = _renderer.material;
        }

        public void Initialize(Transform targetKey, float startY, float length, Color color, float targetY)
        {
            TargetY = targetY + (length / 2f) + _targetOffset.y;
            
            var position = targetKey.position + _targetOffset;
            position.y = startY + (length / 2f);
            transform.position = position;
            
            var scale = transform.localScale;
            scale.y = length;
            transform.localScale = scale;
            
            if (_materialInstance != null)
                _materialInstance.color = color;
        }

        public void StartFalling(float fallSpeed)
        {
            var distance = transform.position.y - TargetY;
            if (distance <= 0f) return;
            
            var duration = distance / fallSpeed;
            
            _fallTween = transform.DOMoveY(TargetY, duration).SetEase(Ease.Linear).OnComplete(HandleMiss);
        }

        public void HandleHit()
        {
            KillTweens();
            
            if (_materialInstance != null)
            {
                _fadeTween = _materialInstance.DOFade(0f, FADE_DURATION).OnComplete(() => Destroy(gameObject));
                return;
            }
            
            Destroy(gameObject);
        }

        private void HandleMiss()
        {
            KillTweens();
            
            if (_materialInstance != null)
            {
                _fadeTween = _materialInstance.DOFade(0f, FADE_DURATION).OnComplete(() => Destroy(gameObject));
                return;
            }
            
            Destroy(gameObject);
        }

        private void KillTweens()
        {
            if (_fallTween != null && _fallTween.IsActive())
                _fallTween.Kill();
                
            if (_fadeTween != null && _fadeTween.IsActive())
                _fadeTween.Kill();
        }
        
        private void OnDestroy()
        {
            KillTweens();
            
            if (_materialInstance != null)
                Destroy(_materialInstance);
        }
    }
}