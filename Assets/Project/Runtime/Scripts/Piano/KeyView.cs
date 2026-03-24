using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts.Piano
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class KeyView : MonoBehaviour
    {
        public static event Action<int> OnNotePlayed;

        public int MidiNote { get; private set; }
        public bool IsWhiteKey { get; private set; }
        public string NoteName { get; private set; }
        public string LabelText { get; private set; }

        private AudioSource _audioSource;
        private Renderer _renderer;
        private Vector3 _originalPosition;
        private Color _defaultColor;
        private Color _originalColor;
        private Color _pressedColor;
        private bool _isPlaying;
        private bool _hasCustomColor;
        private float _pressDepth;
        private GameObject _currentLabel;
        
        private const float WHITE_KEY_PRESS_DEPTH = 0.01f;
        private const float BLACK_KEY_PRESS_DEPTH = 0.005f;
        private const float ANIMATION_DURATION = 0.05f;
        private const float DARKEN_FACTOR = 0.8f;
        private const int LEFT_MOUSE_BUTTON = 0;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _renderer = GetComponent<Renderer>();
            
            if (_renderer != null)
                _defaultColor = _renderer.material.color;
        }

        private void Update()
        {
            if (!_isPlaying) return;
            
            if (Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON))
                ReleaseKey();
        }

        public void Initialize(int midiNote, bool isWhiteKey, string noteName, string labelText)
        {
            MidiNote = midiNote;
            IsWhiteKey = isWhiteKey;
            NoteName = noteName;
            LabelText = labelText;
            
            _originalPosition = transform.localPosition;
            
            if (isWhiteKey)
                _pressDepth = WHITE_KEY_PRESS_DEPTH;
            else
                _pressDepth = BLACK_KEY_PRESS_DEPTH;
            
            var clip = PianoSoundGenerator.CreateTone(midiNote);
            if (clip == null) return;
            
            _audioSource.clip = clip;
            _audioSource.playOnAwake = false;
        }

        public void SetHighlight(bool applyColor, Color customColor, GameObject labelPrefab = null)
        {
            _hasCustomColor = applyColor;
            
            if (_renderer != null)
            {
                if (_hasCustomColor)
                    _originalColor = customColor;
                else
                    _originalColor = _defaultColor;
                    
                _renderer.material.color = _originalColor;
                _pressedColor = new Color(_originalColor.r * DARKEN_FACTOR, _originalColor.g * DARKEN_FACTOR, _originalColor.b * DARKEN_FACTOR, _originalColor.a);
            }

            if (_currentLabel != null)
            {
                Destroy(_currentLabel);
                _currentLabel = null;
            }

            if (_hasCustomColor && labelPrefab != null)
            {
                _currentLabel = Instantiate(labelPrefab, transform);
                var textComponent = _currentLabel.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = LabelText;
            }
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

            OnNotePlayed?.Invoke(MidiNote);
        }
        
        private void ReleaseKey()
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