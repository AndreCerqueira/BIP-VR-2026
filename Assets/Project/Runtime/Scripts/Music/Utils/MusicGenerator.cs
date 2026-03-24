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
        
        private const float QUARTER = 1f;
        private const float HALF = 2f;

        [Button]
        public void Generate()
        {
            if (_musicData == null) return;

            var allNotes = new List<SheetNote>();

            allNotes.AddRange(GetPartA());
            allNotes.AddRange(GetPartB());
            
            allNotes.AddRange(GetPartC());
            allNotes.AddRange(GetPartC());
            
            allNotes.AddRange(GetPartA());
            allNotes.AddRange(GetPartB());

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

        private IEnumerable<SheetNote> GetPartA()
        {
            return new List<SheetNote>
            {
                new SheetNote(C4, QUARTER), new SheetNote(C4, QUARTER),
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(A4, QUARTER), new SheetNote(A4, QUARTER),
                new SheetNote(G4, HALF)
            };
        }

        private IEnumerable<SheetNote> GetPartB()
        {
            return new List<SheetNote>
            {
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, QUARTER), new SheetNote(D4, QUARTER),
                new SheetNote(C4, HALF)
            };
        }

        private IEnumerable<SheetNote> GetPartC()
        {
            return new List<SheetNote>
            {
                new SheetNote(G4, QUARTER), new SheetNote(G4, QUARTER),
                new SheetNote(F4, QUARTER), new SheetNote(F4, QUARTER),
                new SheetNote(E4, QUARTER), new SheetNote(E4, QUARTER),
                new SheetNote(D4, HALF)
            };
        }
    }
}