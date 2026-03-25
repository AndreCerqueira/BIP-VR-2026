using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Project.Runtime.Scripts.Music.Utils;
using Project.Runtime.Scripts.Piano;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class MusicGameplayManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MusicSheetContainer _sheetContainer;
        [SerializeField] private NoteColorSchemeSO _colorScheme;

        [Header("Timing Settings")]
        [SerializeField] private int _bpm = 120;

        [Header("Piano Glow Settings")]
        [SerializeField] private float _glowDuration = 0.5f;
        [SerializeField] private float _minGlowIntensity = 0.5f;
        [SerializeField] private float _maxGlowIntensity = 3.5f;

        private int _currentNoteIndex;
        private IReadOnlyList<SheetNoteView> _notes;

        private void Start()
        {
            if (_sheetContainer == null || _colorScheme == null) return;

            _sheetContainer.BuildSheet();
            _notes = _sheetContainer.AllNotes;
            
            InitializeNotes();
            HighlightAndAnimateCurrentNote();
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

        private void HighlightAndAnimateCurrentNote()
        {
            if (_notes == null) return;

            while (_currentNoteIndex < _notes.Count && _notes[_currentNoteIndex].IsRest)
                _currentNoteIndex++;

            if (_currentNoteIndex >= _notes.Count) return;

            var currentNote = _notes[_currentNoteIndex];
            
            var baseName = MidiHelper.MidiToBaseNoteName(currentNote.MidiNote);
            var targetColor = _colorScheme.GetColorFromName(baseName);
            
            currentNote.SetColor(targetColor);
            currentNote.StartIdleAnimation();

            if (KeyView.ActiveKeys.TryGetValue(currentNote.MidiNote, out var key))
            {
                var secondsPerBeat = 60f / _bpm;
                var realDurationInSeconds = currentNote.Duration * secondsPerBeat;
                
                key.ExpectedDuration = realDurationInSeconds;
                key.IsExpectedNote = true;
                key.StartGlow(_glowDuration, _minGlowIntensity, _maxGlowIntensity);
            }
        }

        private void HandleNotePlayed(int midiNote)
        {
            if (_notes == null || _currentNoteIndex >= _notes.Count) return;

            var currentNote = _notes[_currentNoteIndex];
            if (currentNote.IsRest) return;

            if (currentNote.MidiNote == midiNote)
            {
                currentNote.StopIdleAnimation();
                currentNote.SetColor(_colorScheme.DefaultColor);
                
                if (KeyView.ActiveKeys.TryGetValue(midiNote, out var key))
                {
                    key.StopGlow();
                    key.IsExpectedNote = false;
                }
                
                AdvanceToNextNote();
            }
        }

        private void AdvanceToNextNote()
        {
            _currentNoteIndex++;
            HighlightAndAnimateCurrentNote();
        }
    }
}