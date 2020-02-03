using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Figure;
using Assets.Scripts.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ClickEventController : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        private Vector3 clickDownPosition;
        private FigureController clickFigure;

        private ClickEventControllerStates clickEventControllerState = ClickEventControllerStates.None;

        public void SetCatchClicksState(ClickEventControllerStates state)
        {
            StartCoroutine(WaitEndFrame(() =>
            {
                clickEventControllerState = state;
            }));
        }

        IEnumerator WaitEndFrame(Action endAction)
        {
            yield return new WaitForEndOfFrame();
            endAction?.Invoke();
        }

        void LateUpdate()
        {
            if (clickEventControllerState == ClickEventControllerStates.None) return;

            if (Input.GetButtonDown("Fire1"))
            {
                if (IsPointerOverUIElement())return;
                
                clickDownPosition = GetMousePos();

                clickFigure = CheckRayCast(GetMousePos(), Vector3.forward);
                clickFigure?.SetRotate(true);
                clickEventControllerState = ClickEventControllerStates.WaitUnpress;
            }


            if (Input.GetButtonUp("Fire1"))
            {
                if (clickFigure != null)
                {
                    clickFigure.SetRotate(false);
                }

                if (clickEventControllerState == ClickEventControllerStates.WaitUnpress)
                {
                    var clickUpPosition = GetMousePos();

                    if (Vector3.Distance(clickDownPosition, clickUpPosition) < 0.001f)
                    {
                        PointCut(clickDownPosition);
                    }
                    else
                    {
                        //Закоментил. тк для косой разрезающей не до конца реализован алгоритм.
                        //SlideCut(clickDownPosition, clickUpPosition);
                    }
                }

                clickEventControllerState = ClickEventControllerStates.CatchClicks;
            }
        }

        private FigureController CheckRayCast(Vector3 point, Vector3 vector)
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(point, vector, 100.0F, layerMask);
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.Log("!!!!");
                RaycastHit hit = hits[i];
                var figure = hit.transform.GetComponent<FigureController>();
                if (figure != null) return figure;
            }

            return null;
        }


        private void PointCut(Vector3 clickPoint)
        {
            MeshCutter.CutMesh(MainManager.FigureController.MeshFilter, new Plane(Vector3.right, clickPoint));
            MainManager.FigureController.UpdateMeshRenderer();
        }

        private void SlideCut(Vector3 point1, Vector3 point2)
        {
            var condition = point2.y > point1.y;
            MeshCutter.CutMesh(MainManager.FigureController.MeshFilter,
                new Plane(condition ? point1 : point2, condition ? point2 : point1,
                    new Vector3(point2.x, point2.y, point2.z + 1)));
            MainManager.FigureController.UpdateMeshRenderer();
        }


        public static Vector3 GetMousePos()
        {
            var clickPosition = Input.mousePosition;
            return Camera.main.ScreenToWorldPoint(clickPosition);
        }

        public static Vector3 GetMousePosOnFlat()
        {
            var clickPosition = Input.mousePosition;
            clickPosition = Camera.main.ScreenToWorldPoint(clickPosition);
            clickPosition.z = MainManager.Settings.ZFlat;
            return clickPosition;
        }


        public static bool IsPointerOverUIElement()
        {
            var eventData = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            return raycastResults.Exists(el => el.gameObject.layer == LayerMask.NameToLayer("UI"));
        }
    }
}
