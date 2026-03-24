using DG.Tweening;
using Project.Runtime.Scripts.Music.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Music
{
    public class SheetNoteView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private SheetNote _data;
        private Vector2 _baseSize;
        private TMP_Text _label;
        
        private const float TWEEN_DURATION = 0.2f;

        public int MidiNote => _data?.MidiNote ?? -1;
        public bool IsRest => _data?.IsRest ?? true;

        private void Awake()
        {
            if (_image == null) return;
            
            _baseSize = _image.rectTransform.sizeDelta;
        }

        public void Initialize(SheetNote data, TMP_Text label, float widthMultiplier = 1f, float heightMultiplier = 1f)
        {
            _data = data;
            _label = label;
            
            UpdateSprite();
            
            if (_image != null)
                _image.rectTransform.sizeDelta = new Vector2(_baseSize.x * widthMultiplier, _baseSize.y * heightMultiplier);
        }

        public void SetColor(Color targetColor)
        {
            if (_image != null)
            {
                _image.DOKill();
                _image.DOColor(targetColor, TWEEN_DURATION);
            }

            if (_label == null) return;
            
            _label.DOKill();
            _label.DOColor(targetColor, TWEEN_DURATION);
        }

        private void UpdateSprite()
        {
            if (_image == null) return;
            if (NotationSpriteProvider.Instance == null) return;

            _image.sprite = NotationSpriteProvider.Instance.GetSprite(_data.Duration, _data.IsRest);
        }
    }
}