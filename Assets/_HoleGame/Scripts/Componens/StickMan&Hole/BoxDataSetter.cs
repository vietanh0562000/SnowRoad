namespace HoleBox
{
    using UnityEngine;

    public abstract class BoxDataSetter<T> : MonoBehaviour, IDataSetter where T : BoxData
    {
        private T _data;

        public T Data => _data;

        public void SetData(BoxData boxData)
        {
            _data = (T)boxData;
            
            SetData(_data);
        }

        public abstract void SetData(T data);
    }
}