using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace FPSCamera.Utils
{
    public static class MathUtils
    {
        public struct Range
        {
            public Range(float min = float.MinValue, float max = float.MaxValue)
            {
                this.min = float.IsNaN(min) ? float.MinValue : min;
                this.max = float.IsNaN(max) ? float.MaxValue :
                                              max < min ? min : max;
            }

            public float min, max;
            public float Size => max - min;
        }
        public static bool AlmostEquals(this float a, float b, float error = 1 / 32f)
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
        }
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
        public static float DistanceTo(this Vector3 pos, Vector3 target) => Vector3.Magnitude(new Vector3(target.x - pos.x, target.y - pos.y, target.z - pos.z));

        private static readonly System.Random _random = new System.Random();
        public static T GetRandomOne<T>(this IEnumerable<T> list) => list.Any() ? list.ElementAt(_random.Next(list.Count())) : default;

    }
}
