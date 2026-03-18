using UnityEngine;

namespace Project.Runtime.Scripts
{
    public class PianoBuilder : MonoBehaviour
    {
        [Header("Key Containers")]
        [SerializeField] private Transform _whiteKeysParent;
        [SerializeField] private Transform _blackKeysParent;

        private const int STARTING_MIDI_NOTE = 21; // 33
        private const int WHITE_KEYS_COUNT = 60;
        private const int BLACK_KEYS_COUNT = 42;

        private readonly string[] _whiteNoteNames = { "A", "B", "C", "D", "E", "F", "G" };
        private readonly int[] _whiteNoteOffsets = { 0, 2, 3, 5, 7, 8, 10 };

        private readonly string[] _blackNoteNames = { "As", "Cs", "Ds", "Fs", "Gs" };
        private readonly int[] _blackNoteOffsets = { 1, 4, 6, 9, 11 };

        private void Awake()
        {
            if (_whiteKeysParent == null) return;
            if (_blackKeysParent == null) return;
        
            BuildKeys(_whiteKeysParent, WHITE_KEYS_COUNT, _whiteNoteNames, _whiteNoteOffsets, 7);
            BuildKeys(_blackKeysParent, BLACK_KEYS_COUNT, _blackNoteNames, _blackNoteOffsets, 5);
        }

        private void BuildKeys(Transform parent, int expectedCount, string[] names, int[] offsets, int notesPerOctave)
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
            
                var midiNote = STARTING_MIDI_NOTE + (octave * 12) + midiOffset;
            
                keyTransform.gameObject.name = $"Key_{noteName}{octave}";
            
                var keyView = keyTransform.gameObject.AddComponent<KeyView>();
            
                keyView.Initialize(midiNote);
            }
        }
    }
}