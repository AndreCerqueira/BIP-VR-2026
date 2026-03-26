using System.Collections.Generic;
using Project.Runtime.Scripts.Leveling;
using Project.Runtime.Scripts.Music.Data;
using Project.Runtime.Scripts.Music.Utils;
using Project.Runtime.Scripts.Piano;
using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class MusicGameplayManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PianoBuilder _pianoBuilder;
        [SerializeField] private MusicSheetContainer _sheetContainer;
        [SerializeField] private NoteColorSchemeSO _colorScheme;
        [SerializeField] private FallingNoteView _fallingNotePrefab;
        [SerializeField] private Transform _fallingNotesContainer;
        [SerializeField] private TMP_Text _scoreText;

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
        [SerializeField] private float _noteSpacing = 0.05f;

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
        private int _totalPlayableNotes;
        private int _hitNotes;
        private int _combo;

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
            
            if (_pianoBuilder != null)
                _pianoBuilder.LoadRange(levelData.PianoRange);

            _secondsPerBeat = SECONDS_PER_MINUTE / _bpm;
            _sheetContainer.LoadSheet(levelData.SheetMusic);
            _sheetNoteViews = _sheetContainer.AllNotes;

            InitializeNotes();
            BuildTimeline(levelData.SheetMusic);
            UpdateScoreUI();
            HighlightCurrentExpectedNote();
        }

        private void ClearGame()
        {
            _hasGameStarted = false;
            _songTime = 0f;
            _currentNoteIndex = 0;
            _totalPlayableNotes = 0;
            _hitNotes = 0;
            _combo = 0;

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
            
            UpdateScoreUI();
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
                    var fullDurationInSeconds = note.Duration * _secondsPerBeat;
                    var activeDuration = note.IsRest ? fullDurationInSeconds : Mathf.Max(0.01f, fullDurationInSeconds - _noteSpacing);

                    var timing = new NoteTiming
                    {
                        SheetIndex = sheetIndex,
                        MidiNote = note.MidiNote,
                        HitTime = currentTime,
                        Duration = activeDuration,
                        IsRest = note.IsRest,
                        IsProcessed = false
                    };

                    if (!note.IsRest)
                    {
                        _totalPlayableNotes++;
                        
                        if (KeyView.ActiveKeys.TryGetValue(note.MidiNote, out var key))
                        {
                            var fallingNote = Instantiate(_fallingNotePrefab, _fallingNotesContainer);
                            var baseName = MidiHelper.MidiToBaseNoteName(note.MidiNote);
                            var color = _colorScheme.GetColorFromName(baseName);
                            
                            var targetY = key.transform.position.y;
                            var visualLength = (activeDuration * _noteVisualRatio) * _fallSpeedMultiplier;
                            
                            fallingNote.Initialize(key.transform, currentTime, visualLength, color, targetY);
                            fallingNote.UpdatePosition(0f, _fallSpeedMultiplier);
                            
                            timing.FallingView = fallingNote;
                        }
                    }

                    _noteTimings.Add(timing);

                    if (!note.PlayWithNext)
                        currentTime += fullDurationInSeconds;
                        
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

            var indexChanged = false;

            while (_currentNoteIndex < _noteTimings.Count)
            {
                var currentTiming = _noteTimings[_currentNoteIndex];

                if (currentTiming.IsProcessed)
                {
                    _currentNoteIndex++;
                    indexChanged = true;
                    continue;
                }

                if (currentTiming.IsRest)
                {
                    if (_songTime >= currentTiming.HitTime + currentTiming.Duration)
                    {
                        currentTiming.IsProcessed = true;
                        _currentNoteIndex++;
                        indexChanged = true;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (_songTime > currentTiming.HitTime + _hitWindow)
                    {
                        ProcessMiss(currentTiming);
                        _currentNoteIndex++;
                        indexChanged = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (indexChanged)
                HighlightCurrentExpectedNote();
        }

        private void HandleNotePlayed(int midiNote)
        {
            if (!_hasGameStarted)
            {
                for (var i = _currentNoteIndex; i < _noteTimings.Count; i++)
                {
                    var timing = _noteTimings[i];
                    if (timing.IsProcessed || timing.IsRest) continue;
                    
                    if (timing.HitTime > _noteTimings[_currentNoteIndex].HitTime + 0.01f) break;

                    if (timing.MidiNote == midiNote)
                    {
                        _hasGameStarted = true;
                        _songTime = 0f;
                        ProcessHit(timing);
                        return;
                    }
                }
                
                return;
            }

            for (var i = _currentNoteIndex; i < _noteTimings.Count; i++)
            {
                var timing = _noteTimings[i];
                if (timing.IsProcessed || timing.IsRest) continue;

                if (timing.HitTime > _songTime + _hitWindow) break;

                if (Mathf.Abs(_songTime - timing.HitTime) <= _hitWindow && timing.MidiNote == midiNote)
                {
                    ProcessHit(timing);
                    return;
                }
            }
        }

        private void ProcessHit(NoteTiming timing)
        {
            timing.IsProcessed = true;
            _hitNotes++;
            _combo++;
            UpdateScoreUI();
            
            if (timing.FallingView != null)
                timing.FallingView.HandleHit();
                
            ResetCurrentNoteHighlight(timing);
        }

        private void ProcessMiss(NoteTiming timing)
        {
            timing.IsProcessed = true;
            _combo = 0;
            UpdateScoreUI();
            
            if (timing.FallingView != null)
                timing.FallingView.HandleMiss();
                
            ResetCurrentNoteHighlight(timing);
        }

        private void UpdateScoreUI()
        {
            if (_scoreText == null) return;
            
            var percentage = _totalPlayableNotes > 0 ? ((float)_hitNotes / _totalPlayableNotes) * 100f : 0f;
            _scoreText.text = $"Score: {percentage:F1}% | Combo: {_combo}";
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

        private void HighlightCurrentExpectedNote()
        {
            if (_currentNoteIndex >= _noteTimings.Count) return;

            var targetHitTime = -1f;
            
            for (var i = _currentNoteIndex; i < _noteTimings.Count; i++)
            {
                if (!_noteTimings[i].IsRest && !_noteTimings[i].IsProcessed)
                {
                    targetHitTime = _noteTimings[i].HitTime;
                    break;
                }
            }

            if (targetHitTime < 0f) return;

            for (var i = _currentNoteIndex; i < _noteTimings.Count; i++)
            {
                var timing = _noteTimings[i];
                if (timing.IsRest || timing.IsProcessed) continue;

                if (Mathf.Abs(timing.HitTime - targetHitTime) > 0.01f) break;

                if (_sheetNoteViews != null && timing.SheetIndex < _sheetNoteViews.Count)
                {
                    var view = _sheetNoteViews[timing.SheetIndex];
                    var baseName = MidiHelper.MidiToBaseNoteName(timing.MidiNote);
                    var targetColor = _colorScheme.GetColorFromName(baseName);
                    
                    view.SetColor(targetColor);
                    view.StartIdleAnimation();
                }

                if (KeyView.ActiveKeys.TryGetValue(timing.MidiNote, out var key))
                {
                    key.ExpectedDuration = timing.Duration;
                    key.IsExpectedNote = true;
                    key.StartGlow(_glowDuration, _minGlowIntensity, _maxGlowIntensity);
                }
            }
        }
    }
}