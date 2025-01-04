using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera.Utils
{
    public static class MathUtils
    {
        public static bool AlmostEquals(this float a, float b, float error = 0.03125f)
            => Math.Abs(b - a) < error;
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
            public override string ToString() => $"Position: {pos}, Rotation: {rotation}";
        }
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);

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

        public static float DistanceTo(this Vector3 pos, Vector3 target) => Vector3.Distance(pos, target);

        private static readonly System.Random _random = new System.Random();
        public static T GetRandomOne<T>(this IEnumerable<T> list) => list.Any() ? list.ElementAt(_random.Next(list.Count())) : default;
    }
}
