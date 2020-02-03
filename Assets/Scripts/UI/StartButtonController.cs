using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class StartButtonController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        public Button StartButton => startButton;
        public string ButtonText
        {
            get => buttonText.text;
            set => buttonText.text = value;
        }
    }
}
