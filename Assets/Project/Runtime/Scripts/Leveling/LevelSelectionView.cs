using System.Collections.Generic;
using Project.Runtime.Scripts.Music;
using UnityEngine;

namespace Project.Runtime.Scripts.Leveling
{
    public class LevelSelectionView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _container;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private MusicGameplayManager _gameplayManager;

        [Header("Data")]
        [SerializeField] private List<LevelDataSO> _levels = new List<LevelDataSO>();

        private readonly List<LevelRowView> _spawnedRows = new List<LevelRowView>();
        private LevelRowView _selectedRow;

        private void Start()
        {
            PopulateList();
            SelectFirstLevelByDefault();
        }

        private void OnDestroy()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null)
                    row.OnRowClicked -= HandleRowClicked;
            }
        }

        private void PopulateList()
        {
            if (_container == null) return;
            if (_rowPrefab == null) return;

            foreach (var level in _levels)
            {
                var rowObj = Instantiate(_rowPrefab, _container);
                var rowView = rowObj.GetComponent<LevelRowView>();

                if (rowView != null)
                {
                    rowView.Initialize(level);
                    rowView.OnRowClicked += HandleRowClicked;
                    _spawnedRows.Add(rowView);
                }
            }
        }

        private void SelectFirstLevelByDefault()
        {
            if (_spawnedRows.Count == 0) return;
            
            HandleRowClicked(_spawnedRows[0]);
        }

        private void HandleRowClicked(LevelRowView clickedRow)
        {
            if (_selectedRow == clickedRow)
            {
                _selectedRow.SetSelected(false);
                _selectedRow = null;
                
                if (_gameplayManager != null)
                    _gameplayManager.LoadLevel(null);
                    
                return;
            }

            if (_selectedRow != null)
                _selectedRow.SetSelected(false);

            _selectedRow = clickedRow;
            _selectedRow.SetSelected(true);

            if (_gameplayManager != null)
                _gameplayManager.LoadLevel(_selectedRow.LevelData);
        }
    }
}