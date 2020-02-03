using Assets.Scripts.Configs;
using Assets.Scripts.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Tools.Constructor
{
    public  class FigureConstructor : MonoBehaviour
    {
        public UnityAction<List<Vector3>> FinishConstructorAction;

        protected List<Vector3> outVertices;
        protected static bool isDrawingMode;
        protected int drawingPointNumber;
        private List<GameObject> drawingPoints;
        private int pointCount;

        private MessageController messageController => MainManager.MessageController;
        private Settings settings => MainManager.Settings;

        private void Awake()
        {
            Clear();
        }

        public virtual void Clear()
        {
            isDrawingMode = false;
            drawingPointNumber = 0;
            drawingPoints?.ForEach(Destroy);
            drawingPoints?.Clear();
            outVertices?.Clear();
        }

        protected virtual void Stop()
        {
            isDrawingMode = false;
        }

        public  virtual void StartConstructor(int pointCount, UnityAction<List<Vector3>> finishAction)
        {
            if (isDrawingMode)
            {
                Debug.LogError("Constructor already is running");
                return;
            }

            messageController.SendUIMessage($"Mark {pointCount} points.", MessageTypes.Info);

            this.pointCount = pointCount;
            outVertices = new List<Vector3>(pointCount*2);
            drawingPoints  = new List<GameObject>(pointCount);
            FinishConstructorAction = finishAction;
            drawingPointNumber = 0;
            isDrawingMode = true;
        }

        protected bool ConstructorStep(Vector3 point, int step)
        {
            if (!CheckConditions(point))
            {
                messageController.SendUIMessage("New point doesn't meet conditions . Please check your Settings.", MessageTypes.Error);
                return false;
            }

            if (pointCount - 1 < step)
            {
                messageController.SendUIMessage("Error occured. Please Restart App.", MessageTypes.Error);
                Stop();
                return false;
            }

            messageController.Show(false);

            outVertices.Add(point);
            DrawPoint(point);

            if (pointCount - 1 == step)
            {
                messageController.Show(false);
                Stop(); 
                FinishConstructorAction.Invoke(outVertices);
                Clear();
            }

            return true;
        }

        private bool CheckConditions(Vector3 newPoint)
        {
            if (drawingPoints.Count>0 && drawingPoints.Select(x => Vector3.Distance(x.transform.position, newPoint)).Any(xx => xx <= settings.R0|| xx >= settings.R1))
            {
                return false;
            }

            return true;
        }

        protected void DrawPoint(Vector3 pointPosition)
        {
            var point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = pointPosition;
            drawingPoints.Add(point);
        }

        void Update()
        {
            if (!isDrawingMode) return;

            if (Input.GetButtonDown("Fire1"))
            {
                if (ClickEventController.IsPointerOverUIElement()) return;
                if (ConstructorStep(ClickEventController.GetMousePosOnFlat(), drawingPointNumber))
                {
                    drawingPointNumber++;
                }
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

    }
}
