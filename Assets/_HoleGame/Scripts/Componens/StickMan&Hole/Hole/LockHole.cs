using UnityEngine;

namespace HoleBox
{
    using System.Collections;
    using DG.Tweening;

    public class LockHole : MonoBehaviour
    {
        [SerializeField] private GameObject lockObject; // The visual representation of the lock
        [SerializeField] private GameObject keyObject; // The visual representation of the key

        public void Unlock()
        {
            keyObject.transform.DOLocalMove(Vector3.zero + Vector3.up * 2, 0.4f)
                .SetEase(Ease.Linear).OnComplete(() => { StartCoroutine(CoUnlock()); });

            IEnumerator CoUnlock()
            {
                yield return new WaitForSeconds(0.3f);
                lockObject.SetActive(false);
                keyObject.SetActive(false);
            }
        }

        public void SetKeyHole(HoleBoxData dataKeyHole, int id)
        {
            keyObject.GetComponentInChildren<MeshRenderer>().sharedMaterial = GameAssetManager.Instance.GetMaterialEntryById(id).material;

            lockObject.SetActive(true);
            keyObject.SetActive(true);
            keyObject.transform.position = dataKeyHole.GetMiddlePosition() + Vector3.up * 2;
        }
    }
}