using UnityEngine;
using Project.Runtime.Scripts.Music;
using Project.Runtime.Scripts.Music.Data;

namespace Project.Runtime.Scripts.UI
{
    public class SheetNoteView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private SheetNote _data;
        private Vector2 _baseSize;

        private void Awake()
        {
            if (_spriteRenderer == null) return;
            
            _baseSize = _spriteRenderer.size;
        }

        public void Initialize(SheetNote data, float widthMultiplier = 1f, float heightMultiplier = 1f)
        {
            _data = data;
            
            if (_spriteRenderer != null)
                _spriteRenderer.size = new Vector2(_baseSize.x * widthMultiplier, _baseSize.y * heightMultiplier);
            
            if (_data.IsRest)
                HandleRestVisualization();
        }

        private void HandleRestVisualization()
        {
            gameObject.SetActive(!_data.IsRest);
        }
    }
}