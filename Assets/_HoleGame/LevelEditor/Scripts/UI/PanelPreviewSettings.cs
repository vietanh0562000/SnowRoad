
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoleBox
{
    using System;
    using com.ootii.Messages;
    using BasePuzzle.PuzzlePackages.Core;


    public class PanelPreviewSettings : MonoBehaviour
    {
        public ABasePreviewPropertyView[] panels;
        private ABasePreviewPropertyView _current;

        private void Start()
        {
            GameEvent<ALESpawnItem>.Register(LEEvents.ON_UPDATE_PREVIEW, OnUpdatePreview, this);
            GameEvent<ALESpawnItem>.Register(LEEvents.ON_DESTROY_PREVIEW, OnDestroyPreview, this);
        }

        private void OnDestroy()
        {
            GameEvent<ALESpawnItem>.Unregister(LEEvents.ON_UPDATE_PREVIEW, OnUpdatePreview, this);
            GameEvent<ALESpawnItem>.Unregister(LEEvents.ON_DESTROY_PREVIEW, OnDestroyPreview, this);
        }

        private void OnUpdatePreview(ALESpawnItem item)
        {
            _current?.SetVisible(false);
            
            if (item is LEStickManChunk)
            {
                _current = panels[0];
            }
            else if (item is LEHole)
            {
                _current = panels[1];
            }
            else if (item is LEObstacle)
            {
                _current = panels[2];
            }
            else if (item is LETunnel)
            {
                _current = panels[3];
            }
            
            _current?.Init(item);
            _current?.SetVisible(true);
        }
        
        private void OnDestroyPreview(ALESpawnItem item)
        {
            _current?.SetVisible(false);
            _current = null;
        }
    }
}