using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Project.Runtime.Scripts.Music.Utils;
using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class StaffView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _notePrefab;
        [SerializeField] private GameObject _barLinePrefab;
        [SerializeField] private GameObject _labelPrefab;
        [SerializeField] private Transform _container;

        [Header("Spacing Settings")]
        [SerializeField] private float _beatSpacingX = 1.2f;
        [SerializeField] private float _measureSpacingX = 1.0f;
        [SerializeField] private float _stepY = 0.15f;

        [Header("Treble Staff (Clave de Sol)")]
        [SerializeField] private float _trebleStartYOffset = 0f;
        [SerializeField] private float _trebleBarLineOffsetY = -0.5f;

        [Header("Bass Staff (Clave de Fá)")]
        [SerializeField] private float _bassStartYOffset = -3f;
        [SerializeField] private float _bassBarLineOffsetY = -3.5f;

        [Header("Global Alignment")]
        [SerializeField] private float _startXOffset = 0f;
        [SerializeField] private float _globalYOffset = 0f;
        [SerializeField] private float _labelFixedYOffset = 1.5f;

        [Header("Note Sizing")]
        [SerializeField] private float _noteWidthMultiplier = 1f;
        [SerializeField] private float _noteHeightMultiplier = 1f;

        private const int MIDI_C4 = 60;
        private const int REFERENCE_C4_STEP = 28;
        private const int NOTES_PER_OCTAVE = 12;
        private const int STEPS_PER_OCTAVE = 7;
        
        private readonly int[] _scaleSteps = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };

        public IReadOnlyList<SheetNoteView> SetupStaff(IEnumerable<Measure> measures, int beatsPerMeasure)
        {
            var spawnedNotes = new List<SheetNoteView>();
            
            if (beatsPerMeasure <= 0)
                beatsPerMeasure = 4;

            var isGrandStaff = CheckIfGrandStaff(measures);
            var currentX = _startXOffset;

            foreach (var measure in measures)
            {
                var measureWidth = beatsPerMeasure * _beatSpacingX;
                var measureStartX = currentX;
                var currentBeat = 0f;

                foreach (var note in measure.Notes)
                {
                    var noteX = measureStartX + (currentBeat * _beatSpacingX);
                    var noteView = CreateNote(note, noteX);
                    
                    if (noteView != null)
                        spawnedNotes.Add(noteView);
                        
                    if (!note.PlayWithNext)
                        currentBeat += note.Duration;
                }
                
                currentX = measureStartX + measureWidth;
                
                CreateBarLines(currentX, isGrandStaff);
                currentX += _measureSpacingX;
            }
            
            return spawnedNotes;
        }

        private bool CheckIfGrandStaff(IEnumerable<Measure> measures)
        {
            foreach (var measure in measures)
            {
                foreach (var note in measure.Notes)
                {
                    if (!note.IsRest && note.MidiNote < MIDI_C4)
                        return true;
                }
            }
            
            return false;
        }

        private SheetNoteView CreateNote(SheetNote note, float xPos)
        {
            if (_notePrefab == null) return null;

            var noteObj = Instantiate(_notePrefab, _container);
            var yPos = CalculateNoteY(note);
            noteObj.transform.localPosition = new Vector3(xPos, yPos, 0f);
            
            var view = noteObj.GetComponent<SheetNoteView>();
            TMP_Text labelComponent = null;

            if (!note.IsRest && _labelPrefab != null)
                labelComponent = CreatePitchLabel(xPos, note.MidiNote, yPos);

            if (view != null)
                view.Initialize(note, labelComponent, _noteWidthMultiplier, _noteHeightMultiplier);

            return view;
        }

        private TMP_Text CreatePitchLabel(float xPos, int midiNote, float noteYPos)
        {
            var labelObj = Instantiate(_labelPrefab, _container);
            
            var yPos = noteYPos + _labelFixedYOffset;
            labelObj.transform.localPosition = new Vector3(xPos, yPos, 0f);

            var textComponent = labelObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = MidiHelper.MidiToName(midiNote);
                
            return textComponent;
        }

        private void CreateBarLines(float xPos, bool isGrandStaff)
        {
            if (_barLinePrefab == null) return;

            var trebleBar = Instantiate(_barLinePrefab, _container);
            trebleBar.transform.localPosition = new Vector3(xPos, _trebleBarLineOffsetY + _globalYOffset, 0f);

            if (isGrandStaff)
            {
                var bassBar = Instantiate(_barLinePrefab, _container);
                bassBar.transform.localPosition = new Vector3(xPos, _bassBarLineOffsetY + _globalYOffset, 0f);
            }
        }

        private float CalculateNoteY(SheetNote note)
        {
            var isTreble = note.IsRest || note.MidiNote >= MIDI_C4;
            var anchorY = isTreble ? _trebleStartYOffset : _bassStartYOffset;
            
            var stepOffset = note.IsRest ? 0f : (ConvertMidiToStaffStep(note.MidiNote) * _stepY);
            
            return anchorY + _globalYOffset + stepOffset;
        }

        private int ConvertMidiToStaffStep(int midiNote)
        {
            var octave = (midiNote / NOTES_PER_OCTAVE) - 1;
            var noteIndex = midiNote % NOTES_PER_OCTAVE;
            
            var absoluteStep = (octave * STEPS_PER_OCTAVE) + _scaleSteps[noteIndex];
            
            return absoluteStep - REFERENCE_C4_STEP;
        }
    }
}