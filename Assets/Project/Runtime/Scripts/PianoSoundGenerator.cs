using UnityEngine;

namespace Project.Runtime.Scripts
{
    public static class PianoSoundGenerator
    {
        private const int SAMPLE_RATE = 44100;
        private const float REFERENCE_FREQUENCY = 440f;
        private const int REFERENCE_MIDI_NOTE = 69;
        private const float TWO_PI = 6.2831853f;
        private const float MASTER_VOLUME = 0.4f;
        private const float ATTACK_TIME = 0.05f;
        private const float DECAY_RATE = 2.5f;
        private const float HARMONIC_2_AMP = 0.5f;
        private const float HARMONIC_3_AMP = 0.25f;
        private const float HARMONIC_4_AMP = 0.125f;
        private const int CHANNELS = 1;

        public static AudioClip CreateTone(int midiNote, float duration = 2f)
        {
            if (duration <= 0f) return null;

            var frequency = GetFrequencyFromMidi(midiNote);
            var sampleCount = (int)(SAMPLE_RATE * duration);
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var time = i / (float)SAMPLE_RATE;
                
                var wave = Mathf.Sin(TWO_PI * frequency * time);
                wave += HARMONIC_2_AMP * Mathf.Sin(TWO_PI * frequency * 2f * time);
                wave += HARMONIC_3_AMP * Mathf.Sin(TWO_PI * frequency * 3f * time);
                wave += HARMONIC_4_AMP * Mathf.Sin(TWO_PI * frequency * 4f * time);
                wave *= MASTER_VOLUME;

                var envelope = 1f;
                if (time < ATTACK_TIME)
                    envelope = time / ATTACK_TIME;
                else
                    envelope = Mathf.Exp(-(time - ATTACK_TIME) * DECAY_RATE);

                samples[i] = wave * envelope;
            }

            var clip = AudioClip.Create($"Key_{midiNote}", sampleCount, CHANNELS, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            
            return clip;
        }

        private static float GetFrequencyFromMidi(int midiNote)
        {
            return REFERENCE_FREQUENCY * Mathf.Pow(2f, (midiNote - REFERENCE_MIDI_NOTE) / 12f);
        }
    }
}