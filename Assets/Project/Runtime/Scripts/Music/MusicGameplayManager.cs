using System.Collections.Generic;
using Project.Runtime.Scripts.Leveling;
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
        [SerializeField] private FallingNoteView _fallingNotePrefab;
        [SerializeField] private Transform _fallingNotesContainer;

        [Header("Timing Settings")]
        [SerializeField] private int _bpm = 120;

        [Header("Piano Glow Settings")]
        [SerializeField] private float _glowDuration = 0.5f;
        [SerializeField] private float _minGlowIntensity = 0.5f;
        [SerializeField] private float _maxGlowIntensity = 3.5f;

        [Header("Rhythm Settings")]
        [SerializeField] private float _fallSpeedMultiplier = 1f;
        [SerializeField] private float _yOffsetTarget = 0.1f;
        [SerializeField] private float _spawnYOffset = 5f;

        private int _currentNoteIndex;
        private IReadOnlyList<SheetNoteView> _notes;
        private bool _hasGameStarted;
        private float _secondsPerBeat;
        
        private readonly List<FallingNoteView> _activeFallingNotes = new List<FallingNoteView>();
        
        private const float SECONDS_PER_MINUTE = 60f;

        private void OnEnable()
        {
            KeyView.OnNotePlayed += HandleNotePlayed;
        }

        private void OnDisable()
        {
            KeyView.OnNotePlayed -= HandleNotePlayed;
        }

        public void SetBpm(int newBpm)
        {
            if (newBpm <= 0) return;
            
            _bpm = newBpm;
        }

        public void LoadLevel(LevelDataSO levelData)
        {
            ClearFallingNotes();
            _hasGameStarted = false;

            if (_sheetContainer == null) return;

            foreach (var key in KeyView.ActiveKeys.Values)
            {
                key.StopGlow();
                key.IsExpectedNote = false;
            }

            if (levelData == null || levelData.SheetMusic == null)
            {
                _sheetContainer.ClearSheet();
                _notes = null;
                return;
            }

            _secondsPerBeat = SECONDS_PER_MINUTE / _bpm;
            _sheetContainer.LoadSheet(levelData.SheetMusic);
            _notes = _sheetContainer.AllNotes;
            _currentNoteIndex = 0;

            InitializeNotes();
            BuildFallingNotes(levelData.SheetMusic);
            HighlightAndAnimateCurrentNote();
        }

        private void ClearFallingNotes()
        {
            foreach (var note in _activeFallingNotes)
            {
                if (note != null)
                    Destroy(note.gameObject);
            }
            
            _activeFallingNotes.Clear();
        }

        private void BuildFallingNotes(SheetMusicSO sheetMusic)
        {
            if (_fallingNotePrefab == null) return;
            if (_fallingNotesContainer == null) return;

            var currentTime = 2f; 
            var actualFallSpeed = (_bpm / 60f) * _fallSpeedMultiplier;

            foreach (var measure in sheetMusic.Measures)
            {
                foreach (var note in measure.Notes)
                {
                    if (note.IsRest)
                    {
                        currentTime += note.Duration * _secondsPerBeat;
                        continue;
                    }

                    if (!KeyView.ActiveKeys.TryGetValue(note.MidiNote, out var key))
                    {
                        currentTime += note.Duration * _secondsPerBeat;
                        continue;
                    }

                    var fallingNote = Instantiate(_fallingNotePrefab, _fallingNotesContainer);
                    var baseName = MidiHelper.MidiToBaseNoteName(note.MidiNote);
                    var color = _colorScheme.GetColorFromName(baseName);
            
                    var targetY = key.transform.position.y;
                    var distance = currentTime * actualFallSpeed;
                    var startY = targetY + distance;
                    var length = note.Duration * _secondsPerBeat * actualFallSpeed;
            
                    fallingNote.Initialize(key.transform, startY, length, color, targetY);
                    _activeFallingNotes.Add(fallingNote);

                    currentTime += note.Duration * _secondsPerBeat;
                }
            }
        }

        private void StartGame()
        {
            _hasGameStarted = true;
            
            foreach (var fallingNote in _activeFallingNotes)
            {
                if (fallingNote != null)
                    fallingNote.StartFalling(_fallSpeedMultiplier);
            }
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
                var realDurationInSeconds = currentNote.Duration * _secondsPerBeat;
                
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
                if (!_hasGameStarted)
                    StartGame();

                currentNote.StopIdleAnimation();
                currentNote.SetColor(_colorScheme.DefaultColor);
                
                if (KeyView.ActiveKeys.TryGetValue(midiNote, out var key))
                {
                    key.StopGlow();
                    key.IsExpectedNote = false;
                }
                
                HandleFallingNoteHit();
                AdvanceToNextNote();
            }
        }

        private void HandleFallingNoteHit()
        {
            if (_activeFallingNotes.Count == 0) return;
            
            var hitNote = _activeFallingNotes[0];
            if (hitNote != null)
                hitNote.HandleHit();
                
            _activeFallingNotes.RemoveAt(0);
        }

        private void AdvanceToNextNote()
        {
            _currentNoteIndex++;
            HighlightAndAnimateCurrentNote();
        }
    }
}