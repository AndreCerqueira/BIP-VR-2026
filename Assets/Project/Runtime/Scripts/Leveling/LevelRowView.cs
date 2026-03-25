using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Leveling
{
    public class LevelRowView : MonoBehaviour
    {
        public event Action<LevelRowView> OnRowClicked;

        [Header("UI References")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Button _button;

        [Header("Star Difficulty Settings")]
        [SerializeField] private Transform _starContainer;
        [SerializeField] private GameObject _starPrefab;  
        [SerializeField] private Sprite _filledStarSprite;
        [SerializeField] private Sprite _emptyStarSprite;  

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = Color.cyan;

        private const float COLOR_TWEEN_DURATION = 0.2f;
        private const int MAX_DIFFICULTY = 5;

        private readonly List<Image> _starImages = new List<Image>();

        public LevelDataSO LevelData { get; private set; }

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleClick);

            PrewarmStarPool();
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(HandleClick);
        }

        public void Initialize(LevelDataSO data)
        {
            LevelData = data;
            
            if (_nameText != null)
                _nameText.text = data.LevelName;
                
            UpdateStarDisplay(data.Difficulty);
                
            SetSelected(false, true);
        }

        public void SetSelected(bool isSelected, bool isImmediate = false)
        {
            if (_backgroundImage == null) return;

            var targetColor = isSelected ? _selectedColor : _normalColor;

            _backgroundImage.DOKill();

            if (isImmediate)
                _backgroundImage.color = targetColor;
            else
                _backgroundImage.DOColor(targetColor, COLOR_TWEEN_DURATION);
        }

        private void HandleClick()
        {
            OnRowClicked?.Invoke(this);
        }

        private void PrewarmStarPool()
        {
            if (_starContainer == null || _starPrefab == null) return;

            for (int i = 0; i < MAX_DIFFICULTY; i++)
            {
                var starObj = Instantiate(_starPrefab, _starContainer);
                var img = starObj.GetComponent<Image>();
                if (img != null)
                {
                    img.gameObject.SetActive(false);
                    _starImages.Add(img);
                }
            }
        }

        private void UpdateStarDisplay(int difficulty)
        {
            if (_starImages.Count == 0) return;

            int clampedDifficulty = Mathf.Clamp(difficulty, 0, MAX_DIFFICULTY);

            for (int i = 0; i < _starImages.Count; i++)
            {
                _starImages[i].gameObject.SetActive(true);

                if (i < clampedDifficulty)
                {
                    if (_filledStarSprite != null)
                        _starImages[i].sprite = _filledStarSprite;
                }
                else
                {
                    if (_emptyStarSprite != null)
                        _starImages[i].sprite = _emptyStarSprite;
                    else
                        _starImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}