using System.Collections.Generic;
using UnityEngine;

namespace Project.Runtime.Scripts.Music.Data
{
    [CreateAssetMenu(fileName = "NewSheetMusic", menuName = "Music/Sheet Music")]
    public class SheetMusicSO : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private int _beatsPerMeasure = 4;
        [SerializeField] private List<Measure> _measures = new List<Measure>();

        public IReadOnlyList<Measure> Measures => _measures;
        public int BeatsPerMeasure => _beatsPerMeasure;

        public void GenerateMeasures(List<SheetNote> allNotes)
        {
            _measures.Clear();
            var currentMeasure = new Measure();
            var currentBeatCount = 0f;

            foreach (var note in allNotes)
            {
                currentMeasure.AddNote(note);
                
                if (!note.PlayWithNext)
                    currentBeatCount += note.Duration;

                if (currentBeatCount >= _beatsPerMeasure)
                {
                    _measures.Add(currentMeasure);
                    currentMeasure = new Measure();
                    currentBeatCount = 0f;
                }
            }

            if (currentMeasure.Notes.Count > 0)
                _measures.Add(currentMeasure);
        }
    }
}