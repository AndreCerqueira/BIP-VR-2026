using Project.Runtime.Scripts.Music.Data;
using UnityEngine;

namespace Project.Runtime.Scripts.Leveling
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Music/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [SerializeField] private string _levelName;
        [SerializeField, Range(1, 5)] private int _difficulty = 1;
        [SerializeField] private SheetMusicSO _sheetMusic;

        public string LevelName => _levelName;
        public int Difficulty => _difficulty;
        public SheetMusicSO SheetMusic => _sheetMusic;
    }
}