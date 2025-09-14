using FPSCamera.Cam.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera.Utils
{
    public static class MathUtils
    {

        public struct Positioning
        {
            //z:forward y:up x:left
            [XmlElement("Position")]
            public Vector3 pos;
            [XmlElement("Rotation")]
            public Quaternion rotation;
            public Positioning(Vector3 pos, Quaternion rotation)
            {
                this.pos = pos;
                this.rotation = rotation;
            }
            public Positioning(Vector3 pos)
            {
                this.pos = pos;
                rotation = Quaternion.identity;
            }
            public static Positioning MainCameraPositioning => new Positioning(GameCamController.Instance.MainCamera.transform.position, GameCamController.Instance.MainCamera.transform.rotation);
            /// <summary>
            /// Convert to <see cref="ControllerPositioning"/> using by <see cref="CameraController"/> (Orbit Rotation). (May introduce distortion)
            /// </summary>
            public ControllerPositioning ToControllerPositioning()
            {
                var controllerPositioning = new ControllerPositioning();
                var controller = GameCamController.Instance.CameraController;
                var mainCamera = GameCamController.Instance.MainCamera;

                // Calculate the ground height and angle.
                var height = MapUtils.GetMinHeightAt(pos);
                controllerPositioning.CalculateControllerAngle(rotation);

                // Calculate the new size (height difference between the camera height and the ground, with some adjust by angles)
                var newSize = Mathf.Max(0f, pos.y - height)
                    / Mathf.Lerp(0.15f, 1f, Mathf.Sin(Mathf.Abs(controllerPositioning.angle.y) * Mathf.Deg2Rad));
                newSize = newSize.Clamp(controller.m_minDistance, controller.m_maxDistance);

                // Calculate targetAngle if necessary.
                var shouldCalculate =
                    !(ToolManager.instance.m_properties.m_mode.IsFlagSet(ItemClass.Availability.ThemeEditor)
                    || controller.m_unlimitedCamera //This value would be set to true if there's another camera mod, such as ACME.
                    || AccessUtils.GetFieldValue<bool>(controller, "m_cachedFreeCamera")
                    );
                if (shouldCalculate)
                    controllerPositioning.angle = ControllerPositioning.CalculateTargetAngle(controllerPositioning.angle, newSize);

                var newPos = pos;
                // Calculate CameraController position based on the camera's transform position.
                for (int i = 1; i <= 3; i++)
                {
                    height = TerrainManager.instance.SampleRawHeightSmoothWithWater(newPos, true, 2f);
                    newPos.y = height + newSize * 0.05f + 10f;
                    //Calculate distance (Z offset).
                    float distance = newSize
                                * Mathf.Max(0f, 1f - height / controller.m_maxDistance)
                                / Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad);
                    newPos = pos + (rotation * Vector3.forward * distance);
                    // Limit the camera's position to the allowed area.
                    newPos = CameraController.ClampCameraPosition(newPos);
                    if (newPos.sqrMagnitude < 0.0001f) break;
                }
                controllerPositioning.size = Mathf.Abs(newSize - controller.m_targetSize) >= 100f ? newSize : controller.m_targetSize;
                controllerPositioning.pos = Vector3.Distance(newPos, controller.m_targetPosition) >= 10f ? newPos : controller.m_targetPosition;
                controllerPositioning.height = Mathf.Abs(height - controller.m_targetHeight) >= 10f ? height : controller.m_targetHeight;

                return controllerPositioning;
            }
            public override string ToString() => $"Position: {pos}, Rotation: {rotation}";
        }
        public struct ControllerPositioning
        {
            public Vector3 pos;
            public Vector2 angle;
            public float size;
            public float height;
            private static CameraController Controller => GameCamController.Instance.CameraController;
            public static ControllerPositioning Save()
            => new ControllerPositioning
            {
                pos = Controller.m_targetPosition,
                angle = Controller.m_targetAngle,
                size = Controller.m_targetSize,
                height = Controller.m_targetHeight,
            };
            public void Load()
            {
                var traverse = HarmonyLib.Traverse.Create(Controller);

                Controller.m_targetPosition = Controller.m_currentPosition = pos;
                Controller.m_targetAngle = angle;

                var shouldCalculate = 
                    !(ToolManager.instance.m_properties.m_mode.IsFlagSet(ItemClass.Availability.ThemeEditor)
                    || Controller.m_unlimitedCamera
                    || traverse.Field("m_cachedFreeCamera").GetValue<bool>()
                    );

                Controller.m_currentAngle = shouldCalculate ? CalculateCurrentAngle(angle, size) : angle;
                Controller.m_targetSize = Controller.m_currentSize = size;
                Controller.m_targetHeight = Controller.m_currentHeight = height;


                traverse.Field("m_cachedPosition").SetValue(pos);
                traverse.Field("m_cachedAngle").SetValue(angle);
                traverse.Field("m_cachedSize").SetValue(size);
                traverse.Field("m_cachedHeight").SetValue(height);
            }
            /// <summary>
            /// Convert to <see cref="Positioning"/> using by FPC (Local Rotation). (No distortion)
            /// </summary>
            public Positioning ToPositioning()
            {
                var mainCamera = GameCamController.Instance.MainCamera;

                float num = size * Mathf.Max(0f, 1f - height / Controller.m_maxDistance) / Mathf.Tan(Mathf.PI / 180f * mainCamera.fieldOfView);
                var quaternion = FromControllerAngle();
                var newPos = pos + quaternion * new Vector3(0f, 0f, 0f - num);
                newPos.y += CameraController.CalculateCameraHeightOffset(newPos, num);
                newPos = CameraController.ClampCameraPosition(newPos);
                return new Positioning(newPos, quaternion);
            }
            public void CalculateControllerAngle(Quaternion quaternion) => angle = new Vector2(quaternion.eulerAngles.y, quaternion.eulerAngles.x).ClampEulerAngles();
            public Quaternion FromControllerAngle() => Quaternion.AngleAxis(angle.x, Vector3.up) * Quaternion.AngleAxis(angle.y, Vector3.right);
            public static Vector2 CalculateCurrentAngle(Vector2 targetAngle, float size) => new Vector2(targetAngle.x,
                90f - (90f - targetAngle.y) * (Controller.m_maxTiltDistance * 0.5f / (Controller.m_maxTiltDistance * 0.5f + size)));
            public static Vector2 CalculateTargetAngle(Vector2 currentAngle, float size) => new Vector2(currentAngle.x,
                -((180f * size - currentAngle.y * Controller.m_maxTiltDistance - 2f * currentAngle.y * size) / Controller.m_maxTiltDistance));
            public override string ToString() => $"Position: {pos}, Angle: {angle}, Size: {size}, Height: {height}";
        }


        // Usage:
        // eulerAngles.x => Yaw
        // eulerAngles.y => Pitch (as defined by CameraController).
        //
        // Note: 
        // In Unity's Quaternion.eulerAngles:
        // - x => Pitch
        // - y => Yaw
        // - z => Roll
        public static Vector2 ClampEulerAngles(this Vector2 eulerAngles)
        {
            // Clamp yaw and pitch from 0~360 degrees to -180~180 degrees.
            for (int i = 0; i < 2; i++)
                eulerAngles[i] = Mathf.Repeat(eulerAngles[i] + 180f, 360f) - 180f;
            // Clamp pitch to the range of -90~90 degrees.
            eulerAngles.y = Mathf.Clamp(eulerAngles.y, -90f, 90f);
            return eulerAngles;
        }
        public static bool AlmostEquals(this float a, float b, float error = 0.03125f) => Math.Abs(b - a) < error;
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
        public static float DistanceTo(this Vector3 pos, Vector3 target) => Vector3.Distance(pos, target);

        private static readonly System.Random _random = new System.Random();
        public static T GetRandomOne<T>(this IEnumerable<T> list) => list.Any() ? list.ElementAt(_random.Next(list.Count())) : default;
    }
}
