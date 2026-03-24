using System.Collections.Generic;
using Project.Runtime.Scripts.Music;
using Project.Runtime.Scripts.Music.Data;
using Project.Runtime.Scripts.Music.Utils;
using Project.Runtime.Scripts.Piano;
using Project.Runtime.Scripts.UI;
using UnityEngine;

namespace Project.Runtime.Scripts.Gameplay
{
    public class MusicGameplayManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MusicSheetContainer _sheetContainer;
        [SerializeField] private NoteColorSchemeSO _colorScheme;

        private int _currentNoteIndex;
        private IReadOnlyList<SheetNoteView> _notes;

        private void Start()
        {
            if (_sheetContainer == null) return;
            if (_colorScheme == null) return;

            _sheetContainer.BuildSheet();
            _notes = _sheetContainer.AllNotes;
            
            InitializeNotes();
            HighlightCurrentNote();
        }

        private void OnEnable()
        {
            KeyView.OnNotePlayed += HandleNotePlayed;
        }

        private void OnDisable()
        {
            KeyView.OnNotePlayed -= HandleNotePlayed;
        }

        private void InitializeNotes()
        {
            if (_notes == null) return;

            foreach (var note in _notes)
                note.SetColor(_colorScheme.DefaultColor);
        }

        private void HighlightCurrentNote()
        {
            if (_notes == null) return;

            while (_currentNoteIndex < _notes.Count && _notes[_currentNoteIndex].IsRest)
                _currentNoteIndex++;

            if (_currentNoteIndex >= _notes.Count) return;

            var currentNote = _notes[_currentNoteIndex];
            var baseName = MidiHelper.MidiToBaseNoteName(currentNote.MidiNote);
            var targetColor = _colorScheme.GetColorFromName(baseName);
            
            currentNote.SetColor(targetColor);
        }

        private void HandleNotePlayed(int midiNote)
        {
            if (_notes == null) return;
            if (_currentNoteIndex >= _notes.Count) return;

            var currentNote = _notes[_currentNoteIndex];
            
            if (currentNote.IsRest) return;

            if (currentNote.MidiNote == midiNote)
            {
                currentNote.SetColor(_colorScheme.DefaultColor);
                AdvanceToNextNote();
            }
        }

        private void AdvanceToNextNote()
        {
            _currentNoteIndex++;
            HighlightCurrentNote();
        }
    }
}