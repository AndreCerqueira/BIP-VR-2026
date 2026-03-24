using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Runtime.Scripts.Music.Data
{
    [Serializable]
    public class Measure
    {
        [SerializeField] private List<SheetNote> _notes = new List<SheetNote>();

        public IReadOnlyList<SheetNote> Notes => _notes;

        public void AddNote(SheetNote note)
        {
            _notes.Add(note);
        }
    }
}