using UnityEngine;

namespace Assets.Scripts.Configs
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Configs/Create Settings Config")]
    public class Settings : ScriptableObject
    {
        [Range(0,99)]
        [Tooltip("точки тетраэдара не ближе чем r0")]
        [SerializeField] private float r0=0;

        [Range(1, 200)]
        [Tooltip("точки тетраэдара не дальше чем r1")]
        [SerializeField] private float r1=100;

        [Range(1, 10)]
        [Tooltip("Растояние на которое расходятся куски.")]
        [SerializeField] private float r=1;

        [Range(0, 50)] [Tooltip("толщинга многоуглоника")] [SerializeField]
        private float depth=500;

        private readonly float zFlat = 0;

        public float ZFlat => zFlat;
        public float R0 => r0;
        public float R1 => r1;
        public float R => r;
        public float Depth => depth;

        public void OnValidate()
        {
            if (r1 <= r0)
            {
                Debug.LogError("r0 can't be > then r1");
            }
            if (depth<=0)
            {
                Debug.LogError("Set depth > then 0");
            }
        }
    }
}
