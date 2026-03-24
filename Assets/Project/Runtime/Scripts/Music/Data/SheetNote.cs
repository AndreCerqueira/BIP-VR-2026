using System;
using UnityEngine;

namespace Project.Runtime.Scripts.Music.Data
{
    [Serializable]
    public class SheetNote
    {
        [SerializeField] private int _midiNote;
        [SerializeField] private float _duration;
        [SerializeField] private bool _isRest;

        public int MidiNote => _midiNote;
        public float Duration => _duration;
        public bool IsRest => _isRest;

        public SheetNote(int midiNote, float duration, bool isRest = false)
        {
            _midiNote = midiNote;
            _duration = duration;
            _isRest = isRest;
        }
    }
}