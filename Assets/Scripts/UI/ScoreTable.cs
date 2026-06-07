using System.Collections.Generic;
using UnityEngine;
using MonkeyBusiness.Managers;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace MonkeyBusiness.UI
{
    public class ScoreTable : MonoBehaviour
    {
        [SerializeField]
        List<ScoreRow> _scoreRows = new List<ScoreRow>();

        [SerializeField]
        ScrollRect _scroller;

        [SerializeField]
        GameObject _scoreRowPrefab;

        int _visibleEntries;

        [SerializeField]
        RectTransform _content;

        void Awake()
        {
            _visibleEntries = Mathf.RoundToInt((transform as RectTransform).sizeDelta.y / _scoreRowPrefab.GetComponent<RectTransform>().sizeDelta.y);
        }

        void OnEnable()
        {
            //_content.localScale = Vector3.zero;

            Setup();
            AnimateRows();
        }

        public void Setup()
        {
            int i = 0;
            foreach(var entry in GameManager.Scoreboard.Reverse())
            {
                foreach(var data in entry.Value)
                {
                    if(i >= _scoreRows.Count) AddNewRow();
                    Debug.Log("Setting data for row " + i + ": " + data.Name + " - score: " + entry.Key + " - level " + data.Level );
                    _scoreRows[i].SetData(i + 1, data.Name, entry.Key, data.Level);
                }
            }

            _scroller.vertical = _scoreRows.Count > _visibleEntries;
        }

        [Button("Animate Rows")]
        public void AnimateRows()
        {
            StartCoroutine(AnimateRowsCoroutine());
        }

        IEnumerator AnimateRowsCoroutine()
        {
            for(int i = 0; i < _scoreRows.Count; i++)
            {
                Debug.Log("Animating row " + i);
                _scoreRows[i].Animate();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        void AddNewRow()
        {
            var newRowObj = Instantiate(_scoreRowPrefab, _content);
            var newRow = newRowObj.GetComponent<ScoreRow>();
            _scoreRows.Add(newRow);
            if(_scoreRows.Count > _visibleEntries)
            {
                _content.sizeDelta += new Vector2(0, newRow.GetComponent<RectTransform>().sizeDelta.y);
            }
        }
    }
}
