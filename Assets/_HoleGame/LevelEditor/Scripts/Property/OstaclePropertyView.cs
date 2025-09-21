namespace HoleBox
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine.UI;

	public class ObstaclePropertyView : ABasePreviewPropertyView
	{
		[SerializeField] private Toggle _barrierToggle;
		[SerializeField] private Toggle _openToggle;

		private ObstacleData      _obstacleData;
		

		public override void Init(ALESpawnItem item)
		{
			base.Init(item);
			
			_obstacleData       = _current.Data as ObstacleData;
			
			UpdateFollowData();
		}

		private void UpdateFollowData()
		{
			_barrierToggle.SetIsOnWithoutNotify(_obstacleData.IsBarrier);
			
			_openToggle.transform.parent.gameObject.SetActive(_obstacleData.IsBarrier);
			_openToggle.SetIsOnWithoutNotify(_obstacleData.IsOpenBarrier);
			
			_barrierToggle.onValueChanged.RemoveAllListeners();
			_barrierToggle.onValueChanged.AddListener(OnBarrierToggleChanged);
			
			_openToggle.onValueChanged.RemoveAllListeners();
			_openToggle.onValueChanged.AddListener(OnBarrierOpenToggleChanged);
		}
		
		
		private void OnBarrierToggleChanged(bool arg0)
		{
			_obstacleData.IsBarrier = arg0;
			_openToggle.transform.parent.gameObject.SetActive(arg0);
			
			_current.UpdateFollowData();
		}
		
		private void OnBarrierOpenToggleChanged(bool arg0)
		{
			_obstacleData.IsOpenBarrier = arg0;
			_current.UpdateFollowData();
		}
	}
}