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
        [SerializeField] private bool _playWithNext;

        public int MidiNote => _midiNote;
        public float Duration => _duration;
        public bool IsRest => _isRest;
        public bool PlayWithNext => _playWithNext;

        public SheetNote(int midiNote, float duration, bool isRest = false, bool playWithNext = false)
        {
            _midiNote = midiNote;
            _duration = duration;
            _isRest = isRest;
            _playWithNext = playWithNext;
        }
    }
}