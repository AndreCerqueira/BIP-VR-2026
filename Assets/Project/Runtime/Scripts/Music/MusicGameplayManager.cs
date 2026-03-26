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
        [SerializeField] private float _hitWindow = 0.3f;

        [Header("Piano Glow Settings")]
        [SerializeField] private float _glowDuration = 0.5f;
        [SerializeField] private float _minGlowIntensity = 0.5f;
        [SerializeField] private float _maxGlowIntensity = 3.5f;

        [Header("Rhythm Settings")]
        [SerializeField] private float _fallSpeedMultiplier = 1.0f;
        [SerializeField, Range(0.1f, 1f)] private float _noteVisualRatio = 0.8f;

        private class NoteTiming
        {
            public int SheetIndex;
            public int MidiNote;
            public float HitTime;
            public float Duration;
            public bool IsRest;
            public FallingNoteView FallingView;
            public bool IsProcessed;
        }

        private IReadOnlyList<SheetNoteView> _sheetNoteViews;
        private readonly List<NoteTiming> _noteTimings = new List<NoteTiming>();
        
        private bool _hasGameStarted;
        private float _songTime;
        private float _secondsPerBeat;
        private int _currentNoteIndex;

        private const float SECONDS_PER_MINUTE = 60f;
        private const float OFF_SCREEN_CLEANUP_DELAY = 1f;

        private void OnEnable()
        {
            KeyView.OnNotePlayed += HandleNotePlayed;
        }

        private void OnDisable()
        {
            KeyView.OnNotePlayed -= HandleNotePlayed;
        }

        private void Update()
        {
            if (!_hasGameStarted) return;

            _songTime += Time.deltaTime;

            UpdateGameProgress();
        }

        public void SetBpm(int newBpm)
        {
            if (newBpm <= 0) return;
            
            _bpm = newBpm;
        }

        public void LoadLevel(LevelDataSO levelData)
        {
            ClearGame();

            if (_sheetContainer == null) return;

            if (levelData == null || levelData.SheetMusic == null)
            {
                _sheetContainer.ClearSheet();
                _sheetNoteViews = null;
                return;
            }

            _secondsPerBeat = SECONDS_PER_MINUTE / _bpm;
            _sheetContainer.LoadSheet(levelData.SheetMusic);
            _sheetNoteViews = _sheetContainer.AllNotes;

            InitializeNotes();
            BuildTimeline(levelData.SheetMusic);
            HighlightCurrentExpectedNote();
        }

        private void ClearGame()
        {
            _hasGameStarted = false;
            _songTime = 0f;
            _currentNoteIndex = 0;

            foreach (var timing in _noteTimings)
            {
                if (timing.FallingView != null)
                    Destroy(timing.FallingView.gameObject);
            }
            
            _noteTimings.Clear();

            foreach (var key in KeyView.ActiveKeys.Values)
            {
                key.StopGlow();
                key.IsExpectedNote = false;
            }
        }

        private void InitializeNotes()
        {
            if (_sheetNoteViews == null) return;
            
            foreach (var noteView in _sheetNoteViews)
                noteView.SetColor(_colorScheme.DefaultColor);
        }

        private void BuildTimeline(SheetMusicSO sheetMusic)
        {
            if (_fallingNotePrefab == null) return;
            if (_fallingNotesContainer == null) return;

            var currentTime = 0f;
            var sheetIndex = 0;

            foreach (var measure in sheetMusic.Measures)
            {
                foreach (var note in measure.Notes)
                {
                    var durationInSeconds = note.Duration * _secondsPerBeat;

                    var timing = new NoteTiming
                    {
                        SheetIndex = sheetIndex,
                        MidiNote = note.MidiNote,
                        HitTime = currentTime,
                        Duration = durationInSeconds,
                        IsRest = note.IsRest,
                        IsProcessed = false
                    };

                    if (!note.IsRest && KeyView.ActiveKeys.TryGetValue(note.MidiNote, out var key))
                    {
                        var fallingNote = Instantiate(_fallingNotePrefab, _fallingNotesContainer);
                        var baseName = MidiHelper.MidiToBaseNoteName(note.MidiNote);
                        var color = _colorScheme.GetColorFromName(baseName);
                        
                        var targetY = key.transform.position.y;
                        var visualLength = (durationInSeconds * _noteVisualRatio) * _fallSpeedMultiplier;
                        
                        fallingNote.Initialize(key.transform, currentTime, visualLength, color, targetY);
                        fallingNote.UpdatePosition(0f, _fallSpeedMultiplier);
                        
                        timing.FallingView = fallingNote;
                    }

                    _noteTimings.Add(timing);

                    currentTime += durationInSeconds;
                    sheetIndex++;
                }
            }
        }

        private void UpdateGameProgress()
        {
            foreach (var timing in _noteTimings)
            {
                if (timing.FallingView != null)
                {
                    timing.FallingView.UpdatePosition(_songTime, _fallSpeedMultiplier);
                    
                    if (_songTime > timing.HitTime + timing.Duration + OFF_SCREEN_CLEANUP_DELAY)
                    {
                        Destroy(timing.FallingView.gameObject);
                        timing.FallingView = null;
                    }
                }
            }

            if (_currentNoteIndex >= _noteTimings.Count) return;

            var currentTiming = _noteTimings[_currentNoteIndex];

            if (currentTiming.IsRest)
            {
                if (_songTime >= currentTiming.HitTime + currentTiming.Duration)
                {
                    currentTiming.IsProcessed = true;
                    AdvanceToNextNote();
                }
                
                return;
            }

            if (_songTime > currentTiming.HitTime + _hitWindow)
            {
                currentTiming.IsProcessed = true;
                
                if (currentTiming.FallingView != null)
                    currentTiming.FallingView.HandleHit();
                    
                ResetCurrentNoteHighlight(currentTiming);
                AdvanceToNextNote();
            }
        }

        private void HandleNotePlayed(int midiNote)
        {
            if (_currentNoteIndex >= _noteTimings.Count) return;

            var currentTiming = _noteTimings[_currentNoteIndex];
            
            if (currentTiming.IsRest || currentTiming.IsProcessed) return;

            if (!_hasGameStarted)
            {
                if (currentTiming.MidiNote == midiNote)
                {
                    _hasGameStarted = true;
                    _songTime = 0f;
                    ProcessHit(currentTiming);
                }
                
                return;
            }

            if (currentTiming.MidiNote == midiNote && Mathf.Abs(_songTime - currentTiming.HitTime) <= _hitWindow)
                ProcessHit(currentTiming);
        }

        private void ProcessHit(NoteTiming timing)
        {
            timing.IsProcessed = true;
            
            if (timing.FallingView != null)
                timing.FallingView.HandleHit();
                
            ResetCurrentNoteHighlight(timing);
            AdvanceToNextNote();
        }

        private void ResetCurrentNoteHighlight(NoteTiming timing)
        {
            if (_sheetNoteViews != null && timing.SheetIndex < _sheetNoteViews.Count)
            {
                var view = _sheetNoteViews[timing.SheetIndex];
                view.StopIdleAnimation();
                view.SetColor(_colorScheme.DefaultColor);
            }
            
            if (KeyView.ActiveKeys.TryGetValue(timing.MidiNote, out var key))
            {
                key.StopGlow();
                key.IsExpectedNote = false;
            }
        }

        private void AdvanceToNextNote()
        {
            _currentNoteIndex++;
            HighlightCurrentExpectedNote();
        }

        private void HighlightCurrentExpectedNote()
        {
            if (_currentNoteIndex >= _noteTimings.Count) return;

            var currentTiming = _noteTimings[_currentNoteIndex];
            
            if (currentTiming.IsRest)
            {
                AdvanceToNextNote();
                return;
            }

            if (_sheetNoteViews != null && currentTiming.SheetIndex < _sheetNoteViews.Count)
            {
                var view = _sheetNoteViews[currentTiming.SheetIndex];
                var baseName = MidiHelper.MidiToBaseNoteName(currentTiming.MidiNote);
                var targetColor = _colorScheme.GetColorFromName(baseName);
                
                view.SetColor(targetColor);
                view.StartIdleAnimation();
            }

            if (KeyView.ActiveKeys.TryGetValue(currentTiming.MidiNote, out var key))
            {
                key.ExpectedDuration = currentTiming.Duration;
                key.IsExpectedNote = true;
                key.StartGlow(_glowDuration, _minGlowIntensity, _maxGlowIntensity);
            }
        }
    }
}