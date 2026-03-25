using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Runtime.Scripts.Piano
{
    public class PianoBuilder : MonoBehaviour
    {
        [Header("Key Containers")]
        [SerializeField] private Transform _whiteKeysParent;
        [SerializeField] private Transform _blackKeysParent;

        [Header("UI & Visuals")]
        [SerializeField] private GameObject _noteLabelPrefab;
        [SerializeField] private NoteColorSchemeSO _colorScheme;

        private const int STARTING_MIDI_NOTE = 21;
        private const int WHITE_KEYS_COUNT = 60;
        private const int BLACK_KEYS_COUNT = 42;
        private const int NOTES_PER_OCTAVE = 12;

        private readonly string[] _whiteNoteNames = { "A", "B", "C", "D", "E", "F", "G" };
        private readonly int[] _whiteNoteOffsets = { 0, 2, 3, 5, 7, 8, 10 };

        private readonly string[] _blackNoteNames = { "As", "Cs", "Ds", "Fs", "Gs" };
        private readonly int[] _blackNoteOffsets = { 1, 4, 6, 9, 11 };

        private readonly List<KeyView> _allKeys = new List<KeyView>();

        private void Awake()
        {
            if (_whiteKeysParent == null) return;
            if (_blackKeysParent == null) return;
            
            BuildKeys(_whiteKeysParent, WHITE_KEYS_COUNT, _whiteNoteNames, _whiteNoteOffsets, 7, true);
            BuildKeys(_blackKeysParent, BLACK_KEYS_COUNT, _blackNoteNames, _blackNoteOffsets, 5, false);
            
            LoadOneOctave();
        }

        [Button("Load One Octave (C4 - B4)")]
        public void LoadOneOctave()
        {
            UpdateHighlights(60, 71);
        }

        [Button("Load Two Octaves (C4 - B5)")]
        public void LoadTwoOctaves()
        {
            UpdateHighlights(60, 83);
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
                keyView.Initialize(midiNote, isWhiteKey, noteName, $"{noteName}{midiOctave}");
                
                _allKeys.Add(keyView);
            }
        }

        private void UpdateHighlights(int startMidi, int endMidi)
        {
            foreach (var key in _allKeys)
            {
                var isCentral = key.MidiNote >= startMidi && key.MidiNote <= endMidi;
                
                if (isCentral && key.IsWhiteKey)
                {
                    var keyColor = GetColorForNote(key.NoteName);
                    key.SetHighlight(true, keyColor, _noteLabelPrefab);
                }
                else
                    key.SetHighlight(false, Color.white);
            }
        }

        private Color GetColorForNote(string noteName)
        {
            if (_colorScheme == null) return Color.white;
            return _colorScheme.GetColorFromName(noteName);
        }
    }
}