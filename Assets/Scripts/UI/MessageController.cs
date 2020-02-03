using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MessageController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI textMeshPro;
        [SerializeField] private Color infoColor;
        [SerializeField] private Color errorColor;

        public void Show(bool isShow)
        {
            canvasGroup.alpha = isShow ? 1 : 0;
        }

        public void SendUIMessage(string message, MessageTypes messageType)
        {
            textMeshPro.text = message;
            switch (messageType)
            {
                case MessageTypes.Info:
                    textMeshPro.color = infoColor;
                    break;
                case MessageTypes.Error:
                    textMeshPro.color = errorColor;
                    break;
            }
            Show(true);
        }
    }

    public enum MessageTypes
    {
        Info=0,
        Error =1
    }
}
