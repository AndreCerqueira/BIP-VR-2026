using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Music
{
    public class BpmControlView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private MusicGameplayManager _gameplayManager;

        [Header("Settings")]
        [SerializeField] private int _minBpm = 60;
        [SerializeField] private int _maxBpm = 240;
        [SerializeField] private int _defaultBpm = 120;

        private void Start()
        {
            if (_slider != null)
            {
                _slider.minValue = _minBpm;
                _slider.maxValue = _maxBpm;
                _slider.wholeNumbers = true;
                _slider.value = _defaultBpm;
                
                _slider.onValueChanged.AddListener(HandleSliderChanged);
            }
            
            UpdateView(_defaultBpm);
            SetGameplayBpm(_defaultBpm);
        }

        private void OnDestroy()
        {
            if (_slider != null)
                _slider.onValueChanged.RemoveListener(HandleSliderChanged);
        }

        private void HandleSliderChanged(float value)
        {
            var currentBpm = Mathf.RoundToInt(value);
            UpdateView(currentBpm);
            SetGameplayBpm(currentBpm);
        }

        private void UpdateView(int bpm)
        {
            if (_valueText != null)
                _valueText.text = $"{bpm} BPM";
        }

        private void SetGameplayBpm(int bpm)
        {
            if (_gameplayManager == null) return;
            
            _gameplayManager.SetBpm(bpm);
        }
    }
}