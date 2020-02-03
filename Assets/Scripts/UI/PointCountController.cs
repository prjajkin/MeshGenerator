using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class PointCountController : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI countText;

        // Start is called before the first frame update
        void Start()
        {
            countText.text = MainManager.PointCount.ToString();
        }

        public void TryIncrease()
        {
            if(MainManager.instance.SetPointCount(MainManager.PointCount + 1))
                countText.text = MainManager.PointCount.ToString();
        }

        public void TryDecrease()
        {
            if(MainManager.instance.SetPointCount(MainManager.PointCount - 1))
                countText.text = MainManager.PointCount.ToString();
        }
     
    }
}
