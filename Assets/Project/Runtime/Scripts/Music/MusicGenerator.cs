using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
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
        
        private const float QUARTER = 1f;
        private const float HALF = 2f;

        [Button]
        public void Generate()
        {
            if (_musicData == null) return;

            // TwinkleTwinkle
            var notes = new List<SheetNote>
            {
                new SheetNote(C4, QUARTER), new SheetNote(C4, QUARTER),
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(A4, QUARTER), new SheetNote(A4, QUARTER),
                new SheetNote(G4, HALF),
                
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, HALF)
            };
            
            _musicData.GenerateMeasures(notes);
        }
    }
}