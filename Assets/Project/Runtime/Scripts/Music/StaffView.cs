using System.Collections.Generic;
using UnityEngine;
using Project.Runtime.Scripts.Music.Data;

namespace Project.Runtime.Scripts.UI
{
    public class StaffView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _notePrefab;
        [SerializeField] private Transform _container;

        [Header("Spacing Settings")]
        [SerializeField] private float _noteSpacingX = 1.2f;
        [SerializeField] private float _measureSpacingX = 1.0f;
        [SerializeField] private float _stepY = 0.15f;

        [Header("Alignment Offsets")]
        [SerializeField] private float _startXOffset = 0f;
        [SerializeField] private float _startYOffset = 0f;

        [Header("Note Sizing")]
        [SerializeField] private float _noteWidthMultiplier = 1f;
        [SerializeField] private float _noteHeightMultiplier = 1f;

        private const int REFERENCE_MIDI_C4 = 60;

        public void SetupStaff(IEnumerable<Measure> measures)
        {
            var currentX = _startXOffset;

            foreach (var measure in measures)
            {
                foreach (var note in measure.Notes)
                {
                    CreateNote(note, currentX);
                    currentX += _noteSpacingX;
                }
                currentX += _measureSpacingX;
            }
        }

        private void CreateNote(SheetNote note, float xPos)
        {
            var noteObj = Instantiate(_notePrefab, _container);
            var yPos = CalculateNoteY(note);
            noteObj.transform.localPosition = new Vector3(xPos, yPos, 0f);
            
            var view = noteObj.GetComponent<SheetNoteView>();
            if (view != null)
                view.Initialize(note, _noteWidthMultiplier, _noteHeightMultiplier);
        }

        private float CalculateNoteY(SheetNote note)
        {
            if (note.IsRest) return _startYOffset;
            
            var midiDiff = note.MidiNote - REFERENCE_MIDI_C4;
            return _startYOffset + (ConvertMidiToStaffStep(midiDiff) * _stepY);
        }

        private int ConvertMidiToStaffStep(int midiDiff)
        {
            var octave = midiDiff / 12;
            var noteInOctave = Mathf.Abs(midiDiff % 12);
            var steps = new[] { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };
            var baseStep = (octave * 7) + steps[noteInOctave];
            return midiDiff < 0 ? -baseStep : baseStep;
        }
    }
}