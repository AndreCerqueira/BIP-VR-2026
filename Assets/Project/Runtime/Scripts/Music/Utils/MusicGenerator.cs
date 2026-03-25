using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Project.Runtime.Scripts.Music.Utils
{
    public class MusicGenerator : MonoBehaviour
    {
        [SerializeField] private SheetMusicSO _musicData;

        private const int C4 = 60;
        private const int D4 = 62;
        private const int E4 = 64;
        private const int F4 = 65;
        private const int G4 = 67;
        private const int A4 = 69;
        private const int B4 = 71;
        private const int C5 = 72;
        
        private const float QUARTER = 1f;
        private const float HALF = 2f;
        private const float WHOLE = 4f;

        [Button("Generate: Twinkle Twinkle")]
        public void GenerateTwinkleTwinkle()
        {
            if (_musicData == null) return;

            var allNotes = new List<SheetNote>();

            allNotes.AddRange(new[]
            {
                new SheetNote(C4, QUARTER), new SheetNote(C4, QUARTER),
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(A4, QUARTER), new SheetNote(A4, QUARTER),
                new SheetNote(G4, HALF)
            });

            allNotes.AddRange(new[]
            {
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, HALF)
            });

            var partC = new[]
            {
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, HALF)
            };

            allNotes.AddRange(partC);
            allNotes.AddRange(partC);
            
            allNotes.AddRange(new[]
            {
                new SheetNote(C4, QUARTER), new SheetNote(C4, QUARTER),
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(A4, QUARTER), new SheetNote(A4, QUARTER),
                new SheetNote(G4, HALF)
            });

            allNotes.AddRange(new[]
            {
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, HALF)
            });

            _musicData.GenerateMeasures(allNotes);
            SaveMusicData();
        }

        [Button("Generate: Mary Had a Little Lamb")]
        public void GenerateMaryHadALittleLamb()
        {
            if (_musicData == null) return;

            var allNotes = new List<SheetNote>();
            
            allNotes.AddRange(new[]
            {
                new SheetNote(E4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(E4, HALF)
            });

            allNotes.AddRange(new[]
            {
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(D4, HALF),
                new SheetNote(E4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(G4, HALF)
            });

            allNotes.AddRange(new[]
            {
                new SheetNote(E4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER)
            });

            allNotes.AddRange(new[]
            {
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, WHOLE)
            });

            _musicData.GenerateMeasures(allNotes);
            SaveMusicData();
        }

        [Button("Generate: Ode to Joy")]
        public void GenerateOdeToJoy()
        {
            if (_musicData == null) return;

            var allNotes = new List<SheetNote>();

            var partA = new[]
            {
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(F4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(G4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, QUARTER), new SheetNote(C4, QUARTER),
                new SheetNote(D4, QUARTER), new SheetNote(E4, QUARTER)
            };

            allNotes.AddRange(partA);
            allNotes.AddRange(new[] { new SheetNote(E4, HALF), new SheetNote(D4, HALF) });

            allNotes.AddRange(partA);
            allNotes.AddRange(new[] { new SheetNote(D4, HALF), new SheetNote(C4, HALF) });

            _musicData.GenerateMeasures(allNotes);
            SaveMusicData();
        }

        private void SaveMusicData()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(_musicData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}