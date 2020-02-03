using System;
using UnityEngine;

namespace Assets.Scripts.Figure
{
    public class RotateController : MonoBehaviour
    {
        [SerializeField] private float torque;
        [SerializeField] private Rigidbody rb;

        private bool isRotate;
        private Vector3 lastPosition = Vector3.zero;

        public void SetRotate(bool value)
        {
            if (value)
            {
                lastPosition = ClickEventController.GetMousePosOnFlat();
            }
            isRotate = value;
        }


        void FixedUpdate()
        {
            if(!isRotate)return;

            var delta = (ClickEventController.GetMousePosOnFlat() - lastPosition);
            lastPosition = ClickEventController.GetMousePosOnFlat();
            var turn = new Vector3(delta.y, -delta.x,0);
            if (Math.Abs(delta.magnitude) > Mathf.Epsilon)
            {
                MainManager.ClickEventController.SetCatchClicksState(ClickEventControllerStates.Rotate);
                rb.AddTorque(   torque * turn, ForceMode.VelocityChange);
            }
        }
    }
}
