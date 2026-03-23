using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    public class PianoBuilder : MonoBehaviour
    {
        [Header("Key Containers")]
        [SerializeField] private Transform _whiteKeysParent;
        [SerializeField] private Transform _blackKeysParent;

        [Header("UI & Visuals")]
        [SerializeField] private GameObject _noteLabelPrefab;
        [SerializeField] private Color _colorC = Color.green;
        [SerializeField] private Color _colorD = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color _colorE = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color _colorF = Color.yellow;
        [SerializeField] private Color _colorG = Color.red;
        [SerializeField] private Color _colorA = Color.blue;
        [SerializeField] private Color _colorB = new Color(0.6f, 0.3f, 0f);

        private const int STARTING_MIDI_NOTE = 21;
        private const int WHITE_KEYS_COUNT = 60;
        private const int BLACK_KEYS_COUNT = 42;
        private const int CENTRAL_OCTAVE_START = 60;
        private const int CENTRAL_OCTAVE_END = 83;
        private const int NOTES_PER_OCTAVE = 12;

        private readonly string[] _whiteNoteNames = { "A", "B", "C", "D", "E", "F", "G" };
        private readonly int[] _whiteNoteOffsets = { 0, 2, 3, 5, 7, 8, 10 };

        private readonly string[] _blackNoteNames = { "As", "Cs", "Ds", "Fs", "Gs" };
        private readonly int[] _blackNoteOffsets = { 1, 4, 6, 9, 11 };

        private void Awake()
        {
            if (_whiteKeysParent == null) return;
            if (_blackKeysParent == null) return;
            
            BuildKeys(_whiteKeysParent, WHITE_KEYS_COUNT, _whiteNoteNames, _whiteNoteOffsets, 7, true);
            BuildKeys(_blackKeysParent, BLACK_KEYS_COUNT, _blackNoteNames, _blackNoteOffsets, 5, false);
        }

        private void BuildKeys(Transform parent, int expectedCount, string[] names, int[] offsets, int notesPerOctave, bool isWhiteKey)
        {
            var childCount = parent.childCount;
            if (childCount != expectedCount) return;

            for (var i = 0; i < childCount; i++)
            {
                var keyTransform = parent.GetChild(i);
                var octave = i / notesPerOctave;
                var noteIndex = i % notesPerOctave;
                
                var noteName = names[noteIndex];
                var midiOffset = offsets[noteIndex];
                
                var midiNote = STARTING_MIDI_NOTE + (octave * NOTES_PER_OCTAVE) + midiOffset;
                var midiOctave = (midiNote / NOTES_PER_OCTAVE) - 1;
                
                keyTransform.gameObject.name = $"Key_{noteName}{midiOctave}";
                
                var keyView = keyTransform.gameObject.AddComponent<KeyView>();
                var isCentral = midiNote >= CENTRAL_OCTAVE_START && midiNote <= CENTRAL_OCTAVE_END;
                
                if (isCentral && isWhiteKey)
                {
                    var keyColor = GetColorForNote(noteName);
                    keyView.Initialize(midiNote, keyColor, true, isWhiteKey);
                    CreateNoteLabel(keyTransform, $"{noteName}{midiOctave}", keyColor);
                }
                else
                    keyView.Initialize(midiNote, Color.white, false, isWhiteKey);
            }
        }

        private Color GetColorForNote(string noteName)
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
                default: return Color.white;
            }
        }

        private void CreateNoteLabel(Transform parent, string text, Color color)
        {
            if (_noteLabelPrefab == null) return;
            
            var labelInstance = Instantiate(_noteLabelPrefab, parent);
            var textComponent = labelInstance.GetComponentInChildren<TMP_Text>();
            if (textComponent == null) return;
            
            textComponent.text = text;
            //textComponent.color = color;
        }
    }
}