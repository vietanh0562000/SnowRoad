
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoleBox
{
    using BasePuzzle.PuzzlePackages.Core;

    public class PanelMapSettings : MonoBehaviour
    {
        [Header("Ref")] 
        
        [SerializeField] private LEGrid _leGrid;
        [SerializeField] private Camera _camera;
        
        
        [Header( "UI" )]
        
        [SerializeField] private TMP_InputField     _iptRow,  _iptCol;
        [SerializeField] private TMP_Text _equalTxt;
        
        
        [SerializeField] private Button   _buttonCreate, _buttonChange;
        [SerializeField] private TMP_Text _createTxt;
        
        
        [SerializeField] private TMP_Text _currentTool;
        [SerializeField] private TMP_Text _rotateTxt;
        

        private int _numOfRow = 8, _numOfCol = 8;
        
        private int ActualNumRow => _numOfRow * 2;
        private int ActualNumCol => _numOfCol * 2;

        private void Awake()
        {
            UpdateInputfieldsText();
            
            _buttonCreate.onClick.AddListener(ClickBtnCreate);
            _buttonChange.onClick.AddListener(ClickBtnChange);
            
            _iptRow.onValueChanged.AddListener(delegate { OnIptUpdated(); });
            _iptCol.onValueChanged.AddListener(delegate { OnIptUpdated(); });
            
            GameEvent<(bool, bool)>.Register(LEEvents.ON_UPDATE_MAP_LEVEL_EDITOR, ExtendGrid, this);
            GameEvent<(bool, bool)>.Register(LEEvents.ON_DELETE_MAP_LEVEL_EDITOR, DeleteGrid, this);
            
            UpdateOnCreateBtns(true);
        }

        private void UpdateOnCreateBtns(bool value)
        {
            _buttonChange.gameObject.SetActive(!value);
            _createTxt.SetText(value ? "Create" : "Refresh");
        }

        private void UpdateInputfieldsText()
        {
            _iptRow.SetTextWithoutNotify(_numOfRow.ToString());
            _iptCol.SetTextWithoutNotify(_numOfCol.ToString());

            OnIptUpdated();
        }

        private void OnIptUpdated()
        {
            if (!int.TryParse(_iptRow.text, out _numOfRow)) return;
            if (!int.TryParse(_iptCol.text, out _numOfCol)) return;
            
            _equalTxt.text = $"= [{ActualNumCol} , {ActualNumRow}]";
        }

        private void ClickBtnCreate()
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject (null);
            if (_createTxt.text != "Create")
            {
                Refresh();
                return;
            }
            
            CreateAnew();
            UpdateOnCreateBtns(false);
        }

        private void CreateAnew()
        {
            if (!int.TryParse(_iptRow.text, out _numOfRow)) return;
            if (!int.TryParse(_iptCol.text, out _numOfCol)) return;

            _leGrid.CreateGrid(ActualNumCol, ActualNumRow);
            SetupPositions();
            
            GameEvent<PanelMapSettings>.Emit(LEEvents.CLICK_BTN_APPLY_MAP_SETTINGS, this);
            FireEvents();
        }

        private void ExtendGrid((bool, bool) hor_vel)
        {
            _numOfCol += hor_vel.Item1 ? 1 : 0;
            _iptCol.text = _numOfCol.ToString();
            
            _numOfRow    += hor_vel.Item2 ? 1 : 0;
            _iptRow.text =  _numOfRow.ToString();
            _leGrid.CreateGrid(ActualNumCol, ActualNumRow);
            SetupPositions();
        }

        private void DeleteGrid((bool, bool) row_col)
        {
            _numOfCol    -= row_col.Item2 ? 1 : 0;
            _iptCol.text =  _numOfCol.ToString();

            _numOfRow    -= row_col.Item1 ? 1 : 0;
            _iptRow.text =  _numOfRow.ToString();
            _leGrid.CreateGrid(ActualNumCol, ActualNumRow);
            SetupPositions();
        }

        private void FireEvents()
        {
            GameEvent<PanelMapSettings>.Emit(LEEvents.CLICK_BTN_CREATE_ANEW, this);
            GameEvent<PanelMapSettings>.Emit(LEEvents.ON_SETUP_MAP_LEVEL_EDITOR, this);
        }

        private void Refresh()
        {
            CreateAnew();
        }
        
        private void ClickBtnChange()
        {
            if (!int.TryParse(_iptRow.text, out _numOfRow)) return;
            if (!int.TryParse(_iptCol.text, out _numOfCol)) return;
            
            // Change
            ChangeMap();
        }

        private void ClickBtnMakeBound(bool forceClear)
        {
            
        }

        public void SetTool(string toolName, string external)
        {
            _currentTool.text = toolName;
            _rotateTxt.text   = external;
        }

        public void OnLoadOldLevel(LevelData mapData)
        {
            var matrix = mapData.Matrix;
            _numOfCol = matrix.x / 2;
            _numOfRow = matrix.y / 2;
            
            UpdateInputfieldsText();
            UpdateOnCreateBtns(false);
            
            _leGrid.CreateGrid(ActualNumCol, ActualNumRow);
            SetupPositions();
            
            FireEvents();
        }

        private void ChangeMap()
        {
            
        }

        private void SetupPositions()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            var maxNum     = Mathf.Max(ActualNumRow, ActualNumCol);
            var currentPos = _camera.transform.position;

            _camera.transform.position = new Vector3(ActualNumCol * 0.3f, currentPos.y, ActualNumRow / 2f);
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _camera.orthographicSize   = Mathf.Clamp(maxNum * 0.8f, 10f, 35f);
        }
    }
}