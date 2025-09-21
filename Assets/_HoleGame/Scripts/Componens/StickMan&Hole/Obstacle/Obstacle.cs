namespace HoleBox
{
    using UnityEngine;

    public class Obstacle : BoxDataSetter<ObstacleData>
    {
        [SerializeField] private Barrier    _barrier;
        [SerializeField] private GameObject _defaultObstacle;
        public override void SetData(ObstacleData boxData)
        {
            Data.OnUpdateData -= SetVisualBarrier;
            Data.OnUpdateData -= SetVisualBarrier;
            Data.OnUpdateData += SetVisualBarrier;

            _defaultObstacle.SetActive(!boxData.IsBarrier);

            SetVisualBarrier();
        }

        private void SetVisualBarrier()
        {
            if (Data.IsBarrier)
            {
                _barrier.gameObject.SetActive(true);

                if (Data.AlwaysOpen)
                {
                    _barrier.SetBarrier(false);
                    return;
                }

                _barrier.SetBarrier(Data.IsOpenBarrier);
            }
            else
            {
                _barrier.gameObject.SetActive(false);
            }
        }
    }
}