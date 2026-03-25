using UnityEngine;

namespace Project.Runtime.Scripts.Music.Data
{
    [CreateAssetMenu(fileName = "NewNoteColorScheme", menuName = "Music/Note Color Scheme")]
    public class NoteColorSchemeSO : ScriptableObject
    {
        [Header("Note Colors")]
        [SerializeField] private Color _colorC = Color.green;
        [SerializeField] private Color _colorD = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color _colorE = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color _colorF = Color.yellow;
        [SerializeField] private Color _colorG = Color.red;
        [SerializeField] private Color _colorA = Color.blue;
        [SerializeField] private Color _colorB = new Color(0.6f, 0.3f, 0f);

        [Header("Default Colors")]
        [SerializeField] private Color _blackKeyColor = Color.gray;
        [SerializeField] private Color _defaultColor = Color.black;

        public Color DefaultColor => _defaultColor;

        public Color GetColorFromName(string noteName)
        {
            switch (noteName)
            {
                case "C": return _colorC;
                case "D": return _colorD;
                case "E": return _colorE;
                case "F": return _colorF;
                case "G": return _colorG;
                case "A": return _colorA;
                case "B": return _colorB;
                default: return _blackKeyColor;
            }
        }
    }
}