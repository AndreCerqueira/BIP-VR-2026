using System.Collections.Generic;
using Project.Runtime.Scripts.Music.Data;
using Project.Runtime.Scripts.UI;
using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    public class MusicSheetContainer : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SheetMusicSO _musicData;

        [Header("References")]
        [SerializeField] private GameObject _staffPrefab;
        [SerializeField] private Transform _leftPageParent;
        [SerializeField] private Transform _rightPageParent;

        [Header("Layout & Spacing")]
        [SerializeField] private int _measuresPerStaff = 4;
        [SerializeField] private int _staffsPerPage = 3;
        [SerializeField] private float _staffSpacingY = -5f;

        [Header("Global Offsets")]
        [SerializeField] private float _startOffsetX = 0f;
        [SerializeField] private float _startOffsetY = 0f;

        private readonly List<SheetNoteView> _allNotes = new List<SheetNoteView>();
        private bool _isBuilt;

        public IReadOnlyList<SheetNoteView> AllNotes => _allNotes;

        private void Start()
        {
            BuildSheet();
        }

        public void BuildSheet()
        {
            if (_isBuilt) return;
            if (_musicData == null) return;
            if (_leftPageParent == null) return;
            if (_rightPageParent == null) return;

            var totalMeasures = _musicData.Measures;
            var currentMeasureIndex = 0;
            var staffCount = 0;
            var maxStaffs = _staffsPerPage * 2;

            _allNotes.Clear();

            while (currentMeasureIndex < totalMeasures.Count && staffCount < maxStaffs)
            {
                var targetParent = staffCount < _staffsPerPage ? _leftPageParent : _rightPageParent;
                var staffObj = Instantiate(_staffPrefab, targetParent);
                
                var localStaffIndex = staffCount % _staffsPerPage;
                var posX = _startOffsetX;
                var posY = _startOffsetY + (localStaffIndex * _staffSpacingY);
                
                staffObj.transform.localPosition = new Vector3(posX, posY, 0f);
                
                var staffView = staffObj.GetComponent<StaffView>();
                var measuresForThisStaff = new List<Measure>();

                for (var i = 0; i < _measuresPerStaff && currentMeasureIndex < totalMeasures.Count; i++)
                {
                    measuresForThisStaff.Add(totalMeasures[currentMeasureIndex]);
                    currentMeasureIndex++;
                }

                var staffNotes = staffView.SetupStaff(measuresForThisStaff, _musicData.BeatsPerMeasure);
                _allNotes.AddRange(staffNotes);
                
                staffCount++;
            }

            _isBuilt = true;
        }
    }
}