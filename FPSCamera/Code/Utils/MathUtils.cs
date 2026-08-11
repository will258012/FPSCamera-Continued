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
            private const int CoarseSearchSegments = 16;
            private const int RefinementIterations = 20;
            private const int PositionIterations = 8;
            private const int CameraControllerPositionIterations = 3;
            private const float PositionToleranceSqr = 0.0001f;
            private const float GoldenRatio = 0.61803398875f;

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
            public static Positioning MainCameraPositioning => new(GameCamController.Instance.MainCamera.transform.position, GameCamController.Instance.MainCamera.transform.rotation);
            /// <summary>
            /// Convert to <see cref="ControllerPositioning"/> using by <see cref="CameraController"/> (Orbit Rotation). (May introduce distortion)
            /// </summary>
            [AccessUtils.UsedReflection]
            public ControllerPositioning ToControllerPositioning()
            {
                var controllerPositioning = new ControllerPositioning();
                var controller = GameCamController.Instance.CameraController;
                var mainCamera = GameCamController.Instance.MainCamera;

                // FPS camera positioning guarantees zero roll, so its Euler yaw and pitch
                // map directly to CameraController's two rotation axes.
                controllerPositioning.CalculateControllerAngle(rotation);
                var currentAngle = controllerPositioning.targetAngle;
                var controllerRotation = controllerPositioning.FromControllerAngle();
                var candidate = FindBestControllerPositioning(controllerRotation, controller, mainCamera);

                // Convert the rendered pitch back to CameraController's target pitch when
                // the game applies its distance-dependent tilt adjustment.
                var shouldCalculate =
                    !(ToolManager.instance.m_properties.m_mode.IsFlagSet(ItemClass.Availability.ThemeEditor)
                    || controller.m_unlimitedCamera //This value would be set to true if there's another camera mod, such as ACME.
                    || AccessUtils.GetFieldValue<bool>(controller, "m_cachedFreeCamera")
                    );

                controllerPositioning.pos = candidate.position;
                controllerPositioning.currentAngle = currentAngle;
                controllerPositioning.targetAngle = currentAngle;
                if (shouldCalculate)
                    controllerPositioning.targetAngle = ControllerPositioning.CalculateTargetAngle(currentAngle, candidate.size);
                controllerPositioning.size = candidate.size;
                controllerPositioning.height = candidate.height;

                return controllerPositioning;
            }

            private ControllerPositioningCandidate FindBestControllerPositioning(
                Quaternion controllerRotation,
                CameraController controller,
                Camera mainCamera)
            {
                var preferredSize = Mathf.Clamp(controller.m_targetSize, controller.m_minDistance, controller.m_maxDistance);
                var best = EvaluateControllerPositioning(preferredSize, controllerRotation, controller, mainCamera);
                if (best.error < PositionToleranceSqr)
                    return best;
                var range = controller.m_maxDistance - controller.m_minDistance;
                if (range <= 0f)
                    return best;

                var coarseStep = range / CoarseSearchSegments;
                for (int i = 0; i <= CoarseSearchSegments; i++)
                {
                    var size = controller.m_minDistance + coarseStep * i;
                    var candidate = EvaluateControllerPositioning(size, controllerRotation, controller, mainCamera);
                    if (IsBetterCandidate(candidate, best, preferredSize))
                        best = candidate;
                }

                // Refine around the best coarse result. The forward model is normally
                // smooth in this interval; keeping the coarse pass makes terrain steps
                // and collision discontinuities less likely to select a wrong basin.
                var left = Mathf.Max(controller.m_minDistance, best.size - coarseStep);
                var right = Mathf.Min(controller.m_maxDistance, best.size + coarseStep);
                var leftSize = right - (right - left) * GoldenRatio;
                var rightSize = left + (right - left) * GoldenRatio;
                var leftCandidate = EvaluateControllerPositioning(leftSize, controllerRotation, controller, mainCamera);
                var rightCandidate = EvaluateControllerPositioning(rightSize, controllerRotation, controller, mainCamera);

                for (int i = 0; i < RefinementIterations; i++)
                {
                    if (IsBetterCandidate(leftCandidate, best, preferredSize))
                        best = leftCandidate;
                    if (IsBetterCandidate(rightCandidate, best, preferredSize))
                        best = rightCandidate;

                    if (leftCandidate.error <= rightCandidate.error)
                    {
                        right = rightSize;
                        rightSize = leftSize;
                        rightCandidate = leftCandidate;
                        leftSize = right - (right - left) * GoldenRatio;
                        leftCandidate = EvaluateControllerPositioning(leftSize, controllerRotation, controller, mainCamera);
                    }
                    else
                    {
                        left = leftSize;
                        leftSize = rightSize;
                        leftCandidate = rightCandidate;
                        rightSize = left + (right - left) * GoldenRatio;
                        rightCandidate = EvaluateControllerPositioning(rightSize, controllerRotation, controller, mainCamera);
                    }
                }

                if (IsBetterCandidate(leftCandidate, best, preferredSize))
                    best = leftCandidate;
                if (IsBetterCandidate(rightCandidate, best, preferredSize))
                    best = rightCandidate;
                return best;
            }

            private ControllerPositioningCandidate EvaluateControllerPositioning(
                float size,
                Quaternion controllerRotation,
                CameraController controller,
                Camera mainCamera)
            {
                var forward = controllerRotation * Vector3.forward;
                var targetPosition = pos;
                var height = TerrainManager.instance.SampleRawHeightSmoothWithWater(targetPosition, true, 2f);
                var distance = CalculateControllerDistance(size, height, controller, mainCamera);

                // First solve the orbit centre in XZ. Terrain height changes the camera
                // distance, so this is a small fixed-point problem on uneven terrain.
                for (int i = 0; i < PositionIterations; i++)
                {
                    height = TerrainManager.instance.SampleRawHeightSmoothWithWater(targetPosition, true, 2f);
                    distance = CalculateControllerDistance(size, height, controller, mainCamera);
                    var nextTargetPosition = CameraController.ClampCameraPosition(pos + forward * distance);
                    var deltaX = nextTargetPosition.x - targetPosition.x;
                    var deltaZ = nextTargetPosition.z - targetPosition.z;
                    targetPosition.x = nextTargetPosition.x;
                    targetPosition.z = nextTargetPosition.z;
                    if (deltaX * deltaX + deltaZ * deltaZ < PositionToleranceSqr)
                        break;
                }

                // Mirror CameraController.UpdateTargetPosition, including its three-pass
                // map-edge correction. Repeat the whole update only if its final target
                // clamp changed XZ, so the returned state is stable on the next LateUpdate.
                var clampedCameraPosition = pos;
                for (int i = 0; i < PositionIterations; i++)
                {
                    var previousX = targetPosition.x;
                    var previousZ = targetPosition.z;
                    for (int j = 0; j < CameraControllerPositionIterations; j++)
                    {
                        height = TerrainManager.instance.SampleRawHeightSmoothWithWater(targetPosition, true, 2f);
                        targetPosition.y = height + size * 0.05f + controller.m_minDistance / 4f;
                        distance = CalculateControllerDistance(size, height, controller, mainCamera);
                        var cameraPosition = targetPosition - forward * distance;
                        clampedCameraPosition = CameraController.ClampCameraPosition(cameraPosition);
                        var correction = clampedCameraPosition - cameraPosition;
                        targetPosition += correction;
                        if (correction.sqrMagnitude < PositionToleranceSqr)
                            break;
                    }

                    targetPosition.y += CameraController.CalculateCameraHeightOffset(clampedCameraPosition, distance);
                    targetPosition = CameraController.ClampCameraPosition(targetPosition);
                    var deltaX = targetPosition.x - previousX;
                    var deltaZ = targetPosition.z - previousZ;
                    if (deltaX * deltaX + deltaZ * deltaZ < PositionToleranceSqr)
                        break;
                }

                // Mirror CameraController.UpdateTransform to score the actual rendered
                // position rather than comparing controller fields independently.
                var cameraResult = targetPosition - forward * distance;
                cameraResult.y += CameraController.CalculateCameraHeightOffset(cameraResult, distance);
                cameraResult = CameraController.ClampCameraPosition(cameraResult);
                var error = (cameraResult - pos).sqrMagnitude;
                if (float.IsNaN(error) || float.IsInfinity(error))
                    error = float.MaxValue;

                return new ControllerPositioningCandidate
                {
                    position = targetPosition,
                    size = size,
                    height = height,
                    error = error,
                };
            }

            private static float CalculateControllerDistance(
                float size,
                float height,
                CameraController controller,
                Camera mainCamera)
                => size
                    * Mathf.Max(0f, 1f - height / controller.m_maxDistance)
                    / Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad);

            private static bool IsBetterCandidate(
                ControllerPositioningCandidate candidate,
                ControllerPositioningCandidate current,
                float preferredSize)
            {
                const float equalErrorTolerance = 0.000001f;
                if (candidate.error < current.error - equalErrorTolerance)
                    return true;
                if (Mathf.Abs(candidate.error - current.error) > equalErrorTolerance)
                    return false;
                return Mathf.Abs(candidate.size - preferredSize) < Mathf.Abs(current.size - preferredSize);
            }

            private struct ControllerPositioningCandidate
            {
                public Vector3 position;
                public float size;
                public float height;
                public float error;
            }

            public override string ToString() => $"Position: {pos}, Rotation: {rotation}";
        }
        public struct ControllerPositioning
        {
            public Vector3 pos;
            public Vector2? currentAngle;
            public Vector2 targetAngle;
            public float size;
            public float height;
            private static CameraController Controller => GameCamController.Instance.CameraController;
            public static ControllerPositioning Save()
            => new()
            {
                pos = Controller.m_targetPosition,
                currentAngle = Controller.m_currentAngle,
                targetAngle = Controller.m_targetAngle,
                size = Controller.m_targetSize,
                height = Controller.m_targetHeight,
            };
            [AccessUtils.UsedReflection]
            public void Load()
            {
                var traverse = HarmonyLib.Traverse.Create(Controller);

                Controller.m_targetPosition = Controller.m_currentPosition = pos;
                Controller.m_targetAngle = targetAngle;


                if (currentAngle.HasValue)
                    Controller.m_currentAngle = currentAngle.Value;
                else
                {
                    var shouldCalculate =
                        !(ToolManager.instance.m_properties.m_mode.IsFlagSet(ItemClass.Availability.ThemeEditor)
                        || Controller.m_unlimitedCamera
                        || traverse.Field("m_cachedFreeCamera").GetValue<bool>()
                        );
                    Controller.m_currentAngle = (shouldCalculate ? CalculateCurrentAngle(targetAngle, size) : targetAngle);
                }
                Controller.m_targetSize = Controller.m_currentSize = size;
                Controller.m_targetHeight = Controller.m_currentHeight = height;


                traverse.Field("m_cachedPosition").SetValue(pos);
                traverse.Field("m_cachedAngle").SetValue(targetAngle);
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
            public void CalculateControllerAngle(Quaternion quaternion)
                => targetAngle = ClampEulerAngles(
                    new Vector2(quaternion.eulerAngles.y, quaternion.eulerAngles.x));
            public Quaternion FromControllerAngle() => Quaternion.AngleAxis(targetAngle.x, Vector3.up) * Quaternion.AngleAxis(targetAngle.y, Vector3.right);
            public static Vector2 CalculateCurrentAngle(Vector2 targetAngle, float size) => new(targetAngle.x,
                90f - (90f - targetAngle.y) * (Controller.m_maxTiltDistance * 0.5f / (Controller.m_maxTiltDistance * 0.5f + size)));
            public static Vector2 CalculateTargetAngle(Vector2 currentAngle, float size) => new(currentAngle.x,
                -((180f * size - currentAngle.y * Controller.m_maxTiltDistance - 2f * currentAngle.y * size) / Controller.m_maxTiltDistance));
            public override string ToString() => $"Position: {pos}, currentAngle: {currentAngle}, targetAngle: {targetAngle}, Size: {size}, Height: {height}";
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

        private static readonly System.Random _random = new();
        public static T GetRandomOne<T>(this IEnumerable<T> list) => list.Any() ? list.ElementAt(_random.Next(list.Count())) : default;
    }
}