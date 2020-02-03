using Assets.Scripts.Configs;
using Assets.Scripts.Figure;
using Assets.Scripts.Tools;
using Assets.Scripts.Tools.Constructor;
using Assets.Scripts.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class MainManager : MonoBehaviour
    {
        [SerializeField] public FigureController figurePrefab;
        [SerializeField] private MessageController messageController;
        [SerializeField] private Settings settings;
        [SerializeField] private ClickEventController clickEventController;
        [SerializeField] private StartButtonController startButton;

        public static MainManager instance = null;
        public static readonly bool EnableLogging = false;

        public static MessageController MessageController=> instance.messageController;
        public static Settings Settings=> instance.settings;
        public static ClickEventController ClickEventController => instance.clickEventController;
        public static FigureController FigureController => instance.figure;
        public static int PointCount => instance.pointCount;

        private FigureConstructor constructor;
        private FigureController figure;
        private int pointCount = 4;

        void Awake()
        {
            #region instance

            if (instance == null)
            {
                instance = this;
            }
            else if (instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            #endregion
        }
        void Start()
        {
            messageController.SendUIMessage("Set Vertices Count and Click Start", MessageTypes.Info);
            startButton.StartButton.onClick.AddListener(StartConstructor);
            startButton.ButtonText = "Start";
        }

        private void StartConstructor()
        {
            startButton.StartButton.onClick.RemoveAllListeners();
            startButton.StartButton.onClick.AddListener(Reset);
            startButton.ButtonText = "Reset";

            constructor = gameObject.AddComponent<FigureConstructor>();
            constructor.StartConstructor(pointCount, CreateFigure);
        }


        public void SetPointCount(string value)
        {
            SetPointCount(int.Parse(value));
        }
        public bool SetPointCount(int value)
        {
            if (value < 3 || value > 9)
            {
                return false;
            }

            pointCount = value;
            return true;
        }

        private void CreateFigure(List<Vector3> vertices)
        {
            figure  = Instantiate(figurePrefab, Vector3.zero, Quaternion.identity);
            figure.Init(UniversalMeshGenerator.GenerateMeshFrom2dTo3d(vertices,  Settings.Depth));
            clickEventController.SetCatchClicksState(ClickEventControllerStates.CatchClicks);
        }

        public void Reset()
        {
            clickEventController.SetCatchClicksState(ClickEventControllerStates.None);
            constructor.Clear();
            Destroy(figure);
            constructor.StartConstructor(pointCount, CreateFigure);
            ClearConsole();
        }

        static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            clearMethod.Invoke(null, null);
        }

    }
}
