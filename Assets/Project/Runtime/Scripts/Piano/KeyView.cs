using System;
using System.Collections.Generic;
using DG.Tweening;
using Project.Runtime.Scripts.Music;
using Project.Runtime.Scripts.UI;
using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts.Piano
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class KeyView : MonoBehaviour
    {
        public static event Action<int> OnNotePlayed;
        public static readonly Dictionary<int, KeyView> ActiveKeys = new Dictionary<int, KeyView>();

        public int MidiNote { get; private set; }
        public bool IsWhiteKey { get; private set; }
        public string NoteName { get; private set; }
        public string LabelText { get; private set; }
        public float ExpectedDuration { get; set; }
        public bool IsExpectedNote { get; set; }

        private AudioSource _audioSource;
        private Renderer _renderer;
        private Rigidbody _rigidbody;
        private Vector3 _originalPosition;
        private Color _defaultColor;
        private Color _originalColor;
        private Color _pressedColor;
        public bool _isPlaying;
        private bool _hasCustomColor;
        private bool _isShowingIndicator;
        private float _pressDepth;
        private float _currentGlowIntensity;
        private GameObject _currentLabel;
        private Sequence _glowSequence;
        
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        
        private const float WHITE_KEY_PRESS_DEPTH = 0.01f;
        private const float BLACK_KEY_PRESS_DEPTH = 0.005f;
        private const float ANIMATION_DURATION = 0.05f;
        private const float DARKEN_FACTOR = 0.8f;
        private const int LEFT_MOUSE_BUTTON = 0;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _renderer = GetComponent<Renderer>();
            _rigidbody = GetComponent<Rigidbody>();
            
            if (_renderer != null)
                _defaultColor = _renderer.material.color;

            if (_rigidbody != null)
            {
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }
        }

        private void OnDestroy()
        {
            if (ActiveKeys.ContainsKey(MidiNote))
                ActiveKeys.Remove(MidiNote);
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
            
            ActiveKeys[midiNote] = this;
            
            _originalPosition = transform.localPosition;
            
            if (isWhiteKey)
                _pressDepth = WHITE_KEY_PRESS_DEPTH;
            else
                _pressDepth = BLACK_KEY_PRESS_DEPTH;
            
            var clip = PianoSoundGenerator.CreateTone(midiNote);
            if (clip != null)
            {
                _audioSource.clip = clip;
                _audioSource.playOnAwake = false;
            }
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

        public void StartGlow(float duration, float minIntensity, float maxIntensity)
        {
            if (_renderer == null) return;

            StopGlow();

            _renderer.material.EnableKeyword("_EMISSION");
            
            _glowSequence = DOTween.Sequence();
            
            _currentGlowIntensity = minIntensity;
            _renderer.material.SetColor(EmissionColorId, _originalColor * _currentGlowIntensity);

            var tween = DOTween.To(() => _currentGlowIntensity, x => 
                {
                    _currentGlowIntensity = x;
                    _renderer.material.SetColor(EmissionColorId, _originalColor * _currentGlowIntensity);
                }, maxIntensity, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            _glowSequence.Append(tween);
        }

        public void StopGlow()
        {
            if (_glowSequence != null && _glowSequence.IsPlaying())
                _glowSequence.Kill();

            if (_renderer != null)
            {
                _renderer.material.SetColor(EmissionColorId, Color.black);
                _renderer.material.DisableKeyword("_EMISSION");
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

            if (IsExpectedNote)
            {
                _isShowingIndicator = true;
                if (NoteDurationIndicatorView.Instance != null)
                    NoteDurationIndicatorView.Instance.Show(transform, ExpectedDuration, IsWhiteKey);
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

            if (_isShowingIndicator)
            {
                _isShowingIndicator = false;
                if (NoteDurationIndicatorView.Instance != null)
                    NoteDurationIndicatorView.Instance.Hide();
            }
        }
    }
}