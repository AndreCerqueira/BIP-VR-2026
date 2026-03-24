using Project.Runtime.Scripts.Music.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Music
{
    public class SheetNoteView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private SheetNote _data;
        private Vector2 _baseSize;

        private void Awake()
        {
            if (_image == null) return;
            
            _baseSize = _image.rectTransform.sizeDelta;
        }

        public void Initialize(SheetNote data, float widthMultiplier = 1f, float heightMultiplier = 1f)
        {
            _data = data;
            
            UpdateSprite();
            
            if (_image != null)
                _image.rectTransform.sizeDelta = new Vector2(_baseSize.x * widthMultiplier, _baseSize.y * heightMultiplier);
        }

        private void UpdateSprite()
        {
            if (_image == null) return;
            if (NotationSpriteProvider.Instance == null) return;

            _image.sprite = NotationSpriteProvider.Instance.GetSprite(_data.Duration, _data.IsRest);
        }
    }
}