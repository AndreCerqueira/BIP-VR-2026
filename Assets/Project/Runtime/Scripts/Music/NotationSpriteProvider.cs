using Project.Runtime.Scripts.Utils;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class NotationSpriteProvider : Singleton<NotationSpriteProvider>
    {
        [Header("Note Sprites")]
        [SerializeField] private Sprite _wholeNote;
        [SerializeField] private Sprite _halfNote;
        [SerializeField] private Sprite _quarterNote;
        [SerializeField] private Sprite _eighthNote;
        [SerializeField] private Sprite _sixteenthNote;

        [Header("Rest Sprites")]
        [SerializeField] private Sprite _wholeRest;
        [SerializeField] private Sprite _halfRest;
        [SerializeField] private Sprite _quarterRest;
        [SerializeField] private Sprite _eighthRest;
        [SerializeField] private Sprite _sixteenthRest;

        public Sprite GetSprite(float duration, bool isRest)
        {
            if (isRest)
                return GetRestSprite(duration);
            
            return GetNoteSprite(duration);
        }

        private Sprite GetNoteSprite(float duration)
        {
            if (duration >= 4f) return _wholeNote;
            if (duration >= 2f) return _halfNote;
            if (duration >= 1f) return _quarterNote;
            if (duration >= 0.5f) return _eighthNote;
            
            return _sixteenthNote;
        }

        private Sprite GetRestSprite(float duration)
        {
            if (duration >= 4f) return _wholeRest;
            if (duration >= 2f) return _halfRest;
            if (duration >= 1f) return _quarterRest;
            if (duration >= 0.5f) return _eighthRest;
            
            return _sixteenthRest;
        }
    }
}