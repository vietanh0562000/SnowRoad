using UnityEngine;

namespace PuzzleGames
{
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;

    public class VerifyPopup : MonoBehaviour
    {
        private string _overallMessage;
        private bool   _lastValidationResult;

        public TextMeshProUGUI _verifyText;

        public void Close() { gameObject.SetActive(false); }

        public void ShowPopup(bool isValidate, List<CreateLevelValidator.ValidationResult> result)
        {
            gameObject.SetActive(true);

            if (isValidate)
                _verifyText.SetText("Level hợp lệ!");
            else
            {
                var text = "";

                foreach (var r in result)
                {
                    text += ($"<color=#{ColorUtility.ToHtmlStringRGBA(r.IdColor)}><size=200%>■</size></color> ID {r.ContainerId}: {r.ErrorMessage} \n");
                }

                _verifyText.SetText(text);
            }
        }

        public void ShowPopup(string toast)
        {
            gameObject.SetActive(true);
            _verifyText.SetText(toast);
        }
    }
}