using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class KeyView : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Vector3 _originalPosition;
        private bool _isPlaying;
        
        private const float PRESS_DEPTH = 0.005f;
        private const float ANIMATION_DURATION = 0.1f;
        private const float FADE_OUT_DURATION = 0.15f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(int midiNote)
        {
            _originalPosition = transform.localPosition;
            
            var clip = PianoSoundGenerator.CreateTone(midiNote);
            if (clip == null) return;
            
            _audioSource.clip = clip;
            _audioSource.playOnAwake = false;
        }

        private void OnMouseDown()
        {
            _isPlaying = true;
            
            _audioSource.DOKill();
            _audioSource.volume = 1f;
            _audioSource.Play();
            
            transform.DOLocalMoveY(_originalPosition.y - PRESS_DEPTH, ANIMATION_DURATION).SetEase(Ease.OutQuad);
        }

        private void OnMouseUp()
        {
            if (!_isPlaying) return;
            
            _isPlaying = false;
            _audioSource.DOFade(0f, FADE_OUT_DURATION).OnComplete(() => _audioSource.Stop());
            
            transform.DOLocalMoveY(_originalPosition.y, ANIMATION_DURATION).SetEase(Ease.OutQuad);
        }
    }
}