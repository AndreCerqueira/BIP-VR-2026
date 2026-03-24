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
        [SerializeField] private Transform _staffsParent;

        [Header("Layout & Spacing")]
        [SerializeField] private int _measuresPerStaff = 4;
        [SerializeField] private float _staffSpacingY = -5f;

        [Header("Global Offsets")]
        [SerializeField] private float _startOffsetX = 0f;
        [SerializeField] private float _startOffsetY = 0f;

        private void Start()
        {
            if (_musicData == null) return;
            BuildSheet();
        }

        private void BuildSheet()
        {
            var totalMeasures = _musicData.Measures;
            var currentMeasureIndex = 0;
            var staffCount = 0;

            while (currentMeasureIndex < totalMeasures.Count)
            {
                var staffObj = Instantiate(_staffPrefab, _staffsParent);
                
                var posX = _startOffsetX;
                var posY = _startOffsetY + (staffCount * _staffSpacingY);
                staffObj.transform.localPosition = new Vector3(posX, posY, 0f);
                
                var staffView = staffObj.GetComponent<StaffView>();
                var measuresForThisStaff = new List<Measure>();

                for (var i = 0; i < _measuresPerStaff && currentMeasureIndex < totalMeasures.Count; i++)
                {
                    measuresForThisStaff.Add(totalMeasures[currentMeasureIndex]);
                    currentMeasureIndex++;
                }

                staffView.SetupStaff(measuresForThisStaff);
                staffCount++;
            }
        }
    }
}