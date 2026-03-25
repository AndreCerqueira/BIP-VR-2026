using System;

namespace Project.Runtime.Scripts.Music.Utils
{
    public static class MidiHelper
    {
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private static readonly string[] BaseNoteNames = { "C", "C", "D", "D", "E", "F", "F", "G", "G", "A", "A", "B" };

        public static string MidiToName(int midiNote)
        {
            if (midiNote < 0 || midiNote > 127) return "??";

            var octave = (midiNote / 12) - 1;
            var noteIndex = midiNote % 12;

            return NoteNames[noteIndex] + octave;
        }

        public static string MidiToBaseNoteName(int midiNote)
        {
            if (midiNote < 0 || midiNote > 127) return "??";

            var noteIndex = midiNote % 12;

            return BaseNoteNames[noteIndex];
        }
    }
}