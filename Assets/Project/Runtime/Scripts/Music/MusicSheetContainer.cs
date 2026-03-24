using System.Collections.Generic;
using UnityEngine;
using Project.Runtime.Scripts.Music;
using Project.Runtime.Scripts.Music.Data;

namespace Project.Runtime.Scripts.UI
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

        private void Start()
        {
            if (_musicData == null) return;
            if (_leftPageParent == null) return;
            if (_rightPageParent == null) return;
            
            BuildSheet();
        }

        private void BuildSheet()
        {
            var totalMeasures = _musicData.Measures;
            var currentMeasureIndex = 0;
            var staffCount = 0;
            var maxStaffs = _staffsPerPage * 2;

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

                staffView.SetupStaff(measuresForThisStaff, _musicData.BeatsPerMeasure);
                staffCount++;
            }
        }
    }
}