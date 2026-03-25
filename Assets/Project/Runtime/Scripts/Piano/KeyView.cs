using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Piano
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class KeyView : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Renderer _renderer;
        private Vector3 _originalPosition;
        private Color _originalColor;
        private Color _pressedColor;
        public bool _isPlaying;
        private bool _hasCustomColor;
        private float _pressDepth;
        
        private const float WHITE_KEY_PRESS_DEPTH = 0.01f;
        private const float BLACK_KEY_PRESS_DEPTH = 0.005f;
        private const float ANIMATION_DURATION = 0.05f;
        private const float DARKEN_FACTOR = 0.8f;
        private const int LEFT_MOUSE_BUTTON = 0;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            
            if (Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON))
                ReleaseKey();
        }

        public void Initialize(int midiNote, Color customColor, bool applyColor, bool isWhiteKey)
        {
            _originalPosition = transform.localPosition;
            _hasCustomColor = applyColor;
            
            if (isWhiteKey)
                _pressDepth = WHITE_KEY_PRESS_DEPTH;
            else
                _pressDepth = BLACK_KEY_PRESS_DEPTH;
            
            if (_hasCustomColor && _renderer != null)
            {
                _renderer.material.color = customColor;
                _originalColor = customColor;
                _pressedColor = new Color(customColor.r * DARKEN_FACTOR, customColor.g * DARKEN_FACTOR, customColor.b * DARKEN_FACTOR, customColor.a);
            }
            
            var clip = PianoSoundGenerator.CreateTone(midiNote);
            if (clip == null) return;
            
            _audioSource.clip = clip;
            _audioSource.playOnAwake = false;
        }

        private void OnMouseDown()
        {
            PressKey();
        }

        private void OnMouseExit()
        {
            ReleaseKey();
        }

        public void PressKey()
        {
            _isPlaying = true;
            
            _audioSource.DOKill();
            _audioSource.volume = 1f;
            _audioSource.Play();
            
            transform.DOKill();
            transform.DOLocalMoveY(_originalPosition.y - _pressDepth, ANIMATION_DURATION).SetEase(Ease.Linear);
            
            if (_hasCustomColor && _renderer != null)
            {
                _renderer.material.DOKill();
                _renderer.material.DOColor(_pressedColor, ANIMATION_DURATION);
            }
        }
        
        public void ReleaseKey()
        {
            if (!_isPlaying) return;
            
            _isPlaying = false;
            
            transform.DOKill();
            transform.DOLocalMoveY(_originalPosition.y, ANIMATION_DURATION).SetEase(Ease.Linear);
            
            if (_hasCustomColor && _renderer != null)
            {
                _renderer.material.DOKill();
                _renderer.material.DOColor(_originalColor, ANIMATION_DURATION);
            }
        }
    }
}