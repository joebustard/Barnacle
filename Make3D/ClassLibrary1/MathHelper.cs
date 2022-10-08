﻿namespace HullLibrary
{
    public static class MathHelper
    {
        /// <summary>
        /// Degrees to radian constant.
        /// </summary>
        public const float Deg2Rad = PI / 180f;

        /// <summary>
        /// Degrees to radian constant.
        /// </summary>
        public const double Deg2Radd = PId / 180.0;

        /// <summary>
        /// The Pi constant.
        /// </summary>
        public const float PI = 3.14159274f;

        /// <summary>
        /// The Pi constant.
        /// </summary>
        public const double PId = 3.1415926535897932384626433832795;

        /// <summary>
        /// Radians to degrees constant.
        /// </summary>
        public const float Rad2Deg = 180f / PI;

        /// <summary>
        /// Radians to degrees constant.
        /// </summary>
        public const double Rad2Degd = 180.0 / PId;

        /// <summary>
        /// Clamps a value between a minimum and a maximum value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float value, float min, float max)
        {
            return (value >= min ? (value <= max ? value : max) : min);
        }

        /// <summary>
        /// Clamps a value between a minimum and a maximum value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static double Clamp(double value, double min, double max)
        {
            return (value >= min ? (value <= max ? value : max) : min);
        }

        /// <summary>
        /// Clamps the value between 0 and 1.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp01(float value)
        {
            return (value > 0f ? (value < 1f ? value : 1f) : 0f);
        }

        /// <summary>
        /// Clamps the value between 0 and 1.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public static double Clamp01(double value)
        {
            return (value > 0.0 ? (value < 1.0 ? value : 1.0) : 0.0);
        }

        /// <summary>
        /// Returns the maximum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The maximum value.</returns>
        public static int Max(int val1, int val2)
        {
            return (val1 > val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the maximum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The maximum value.</returns>
        public static int Max(int val1, int val2, int val3)
        {
            return (val1 > val2 ? (val1 > val3 ? val1 : val3) : (val2 > val3 ? val2 : val3));
        }

        /// <summary>
        /// Returns the maximum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The maximum value.</returns>
        public static float Max(float val1, float val2)
        {
            return (val1 > val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the maximum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The maximum value.</returns>
        public static float Max(float val1, float val2, float val3)
        {
            return (val1 > val2 ? (val1 > val3 ? val1 : val3) : (val2 > val3 ? val2 : val3));
        }

        /// <summary>
        /// Returns the maximum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The maximum value.</returns>
        public static double Max(double val1, double val2)
        {
            return (val1 > val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the maximum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The maximum value.</returns>
        public static double Max(double val1, double val2, double val3)
        {
            return (val1 > val2 ? (val1 > val3 ? val1 : val3) : (val2 > val3 ? val2 : val3));
        }

        /// <summary>
        /// Returns the minimum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The minimum value.</returns>
        public static int Min(int val1, int val2)
        {
            return (val1 < val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the minimum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The minimum value.</returns>
        public static int Min(int val1, int val2, int val3)
        {
            return (val1 < val2 ? (val1 < val3 ? val1 : val3) : (val2 < val3 ? val2 : val3));
        }

        /// <summary>
        /// Returns the minimum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The minimum value.</returns>
        public static float Min(float val1, float val2)
        {
            return (val1 < val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the minimum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The minimum value.</returns>
        public static float Min(float val1, float val2, float val3)
        {
            return (val1 < val2 ? (val1 < val3 ? val1 : val3) : (val2 < val3 ? val2 : val3));
        }

        /// <summary>
        /// Returns the minimum of two values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <returns>The minimum value.</returns>
        public static double Min(double val1, double val2)
        {
            return (val1 < val2 ? val1 : val2);
        }

        /// <summary>
        /// Returns the minimum of three values.
        /// </summary>
        /// <param name="val1">The first value.</param>
        /// <param name="val2">The second value.</param>
        /// <param name="val3">The third value.</param>
        /// <returns>The minimum value.</returns>
        public static double Min(double val1, double val2, double val3)
        {
            return (val1 < val2 ? (val1 < val3 ? val1 : val3) : (val2 < val3 ? val2 : val3));
        }

        /// <summary>
        /// Calculates the area of a triangle.
        /// </summary>
        /// <param name="p0">The first point.</param>
        /// <param name="p1">The second point.</param>
        /// <param name="p2">The third point.</param>
        /// <returns>The triangle area.</returns>
        public static float TriangleArea(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2)
        {
            var dx = p1 - p0;
            var dy = p2 - p0;
            return dx.Magnitude * ((float)System.Math.Sin(Vector3.Angle(ref dx, ref dy) * Deg2Rad) * dy.Magnitude) * 0.5f;
        }
    }
}