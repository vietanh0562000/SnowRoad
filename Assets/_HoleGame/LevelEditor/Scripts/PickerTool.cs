
using HoleBox;
using UnityEngine;

public class PickerTool : MonoBehaviour, IToolMode
{
	[SerializeField] private DynamicLEAssetInfoUI _leAssetUI;

	private int                _count;

	public void OnInit()
	{
		if (_leAssetUI == null)
		{
			_leAssetUI = FindFirstObjectByType<DynamicLEAssetInfoUI>(FindObjectsInactive.Include);
		}
		
		_leAssetUI.Init();
	}

	public void OnToolEnable()
	{
		_count = 0;
		_leAssetUI.Show(true);
		Debug.Log("Picker Tool Enabled");
	}

	public void OnToolDisable()
	{
		_count = 0;
		_leAssetUI.Show(false);
	}
	
	public void OnUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_count++;
			if (_count > 1) OnEscapeKey();
		}
	}

	public void OnEscapeKey() { GetComponent<LevelEditorManager>().SetDefaultTool(); }

	public string ToolName      => "Picker Tool";
	public string ExternalUsage => "[Space] to escape";
}