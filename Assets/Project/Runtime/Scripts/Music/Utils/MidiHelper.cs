using System;

namespace Project.Runtime.Scripts.Music.Utils
{
    public static class MidiHelper
    {
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string MidiToName(int midiNote)
        {
            if (midiNote < 0 || midiNote > 127) return "??";

            int octave = (midiNote / 12) - 1;
            int noteIndex = midiNote % 12;

            return NoteNames[noteIndex] + octave;
        }
    }
}