using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH.Caress
{
    internal class MoMiCircles
    {
        private bool _circleHelper;

        public Vector2 GetPosition(int ptn, float deg, float step, float intensity, int peak, int range, out float modDeg)
        {
            Vector2 vector;
            //vector = MankoSideToSideFast(deg, step, intensity, out modDeg);
            switch (ptn)
            {
                case 0:
                    vector = CircleClock(deg, step, intensity, out modDeg);
                    break;
                case 1:
                    vector = CircleClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 2:
                    vector = CircleCounterClock(deg, step, intensity, out modDeg);
                    break;
                case 3:
                    vector = CircleCounterClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 4:
                    vector = OvalVerticalClock(deg, step, intensity, out modDeg);
                    break;
                case 5:
                    vector = OvalVerticalClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 6:
                    vector = OvalVerticalCounterClock(deg, step, intensity, out modDeg);
                    break;
                case 7:
                    vector = OvalVerticalCounterClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 8:
                    vector = OvalHorizontalClock(deg, step, intensity, out modDeg);
                    break;
                case 9:
                    vector = OvalHorizontalClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 10:
                    vector = OvalHorizontalCounterClock(deg, step, intensity, out modDeg);
                    break;
                case 11:
                    vector = OvalHorizontalCounterClockAccel(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 12:
                    vector = PartCircle(deg, step, intensity, peak, range, out modDeg);
                    break;
                case 13:
                    vector = MankoVerticalOval(deg, step, intensity, out modDeg);
                    break;
                case 14:
                    vector = MankoCircle(deg, step, intensity, out modDeg);
                    break;
                case 15:
                    vector = MankoHorizontalOval(deg, step, intensity, out modDeg);
                    break;
                case 16:
                    vector = MankoSideToSide(deg, step, intensity, out modDeg);
                    break;
                case 17:
                    vector = MankoSideToSideFast(deg, step, intensity, out modDeg);
                    break;
                default:
                    vector = Vector2.zero;
                    modDeg = deg;
                    break;
            }

            //SensibleH.Logger.LogDebug($"[{ptn}] int[{intensity}] deg[{deg}] step[{step}] pos:[{vector.x}][{vector.y}]");
            return vector;
        }
        private static float GetCos(float deg, float intensity) => Mathf.Cos(deg * Mathf.Deg2Rad) / (2f * intensity) + 0.5f;
        private static float GetSin(float deg, float intensity) => Mathf.Sin(deg * Mathf.Deg2Rad) / (2f * intensity) + 0.5f;
        private static float NormalizeDeg(float deg, int peak)
        {
            if (deg > peak + 180)
                deg = peak - 180 + (deg - (peak + 180));
            else if (deg < peak - 180)
                deg = peak + 180 + (deg - (peak - 180));
            return deg;
        }
        private static Vector2 CircleClock(float deg, float step, float intensity, out float newDeg)
        {
            // 0.
            deg = Mathf.Repeat(deg, 360f);
            deg -= step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity));

        }
        private static Vector2 CircleClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {
            // 1.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg -= step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity));
        }
        private static Vector2 CircleCounterClock(float deg, float step, float intensity, out float newDeg)
        {
            // 2.
            deg = Mathf.Repeat(deg, 360f);
            deg += step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity));

        }
        private static Vector2 CircleCounterClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {

            // 3.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg += step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity));
        }
        private static Vector2 OvalVerticalClock(float deg, float step, float intensity, out float newDeg)
        {
            // 4.
            deg = Mathf.Repeat(deg, 360f);
            deg -= step;
            newDeg = deg;
            //return new Vector2(DividePos(GetCos(deg, intensity), 2f), GetSin(deg, intensity));
            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity));

        }
        private static Vector2 OvalVerticalClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {

            // 5.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg -= step;
            newDeg = deg;
            //return new Vector2(DividePos(GetCos(deg, intensity), 2f), GetSin(deg, intensity));
            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity));
        }
        private static Vector2 OvalVerticalCounterClock(float deg, float step, float intensity, out float newDeg)
        {
            // 6.
            deg = Mathf.Repeat(deg, 360f);
            deg += step;
            newDeg = deg;
            //return new Vector2(DividePos(GetCos(deg, intensity), 2f), GetSin(deg, intensity));
            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity));

        }
        private static Vector2 OvalVerticalCounterClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {

            // 7.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg += step;
            newDeg = deg;
            //return new Vector2(DividePos(GetCos(deg, intensity), 2f), GetSin(deg, intensity));
            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity));
        }
        private static Vector2 OvalHorizontalClock(float deg, float step, float intensity, out float newDeg)
        {
            // 8.
            deg = Mathf.Repeat(deg, 360f);
            deg -= step;
            newDeg = deg;
            //return new Vector2(GetCos(deg, intensity), DividePos(GetSin(deg, intensity), 2f));
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 2f));

        }
        private static Vector2 OvalHorizontalClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {

            // 9.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg -= step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg -= step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 2f));
        }
        private static Vector2 OvalHorizontalCounterClock(float deg, float step, float intensity, out float newDeg)
        {
            // 10.
            deg = Mathf.Repeat(deg, 360f);
            deg += step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 2f));

        }
        private static Vector2 OvalHorizontalCounterClockAccel(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {

            // 11.
            deg = NormalizeDeg(deg, peak);

            if (deg < peak + range && deg > peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((deg - peak) / range));
            else if (deg > peak - range && deg < peak)
                deg += step + step * Mathf.Lerp(1f, 0f, Mathf.Abs((peak - deg) / range));
            else
                deg += step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 2f));
        }
        private Vector2 PartCircle(float deg, float step, float intensity, int peak, int range, out float newDeg)
        {
            // 12.
            deg = NormalizeDeg(deg, peak);


            if (deg < peak - range)
            {
                if (_circleHelper)
                    _circleHelper = false;
                deg += step;
            }
            else if (_circleHelper || deg > peak + range)
            {
                if (!_circleHelper)
                    _circleHelper = true;
                deg -= step;
            }
            else
                deg += step;
            newDeg = deg;
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity));
        }
        private static Vector2 MankoVerticalOval(float deg, float step, float intensity, out float newDeg)
        {
            // 13.
            deg = Mathf.Repeat(deg, 360);
            newDeg = deg += step * intensity;

            //return new Vector2(DividePos(GetCos(deg , intensity), 4f), GetSin(deg, intensity));
            return new Vector2(GetCos(deg, intensity * 4f), GetSin(deg, intensity));
        }
        //private Vector2 MankoBackForth2(float _degrees, float _step, float _intensity, out float _modDegrees)
        //{
        //    // 34.
        //    _modDegrees = _degrees + (_step * 2f);

        //    return new Vector2(GetCos(_degrees, _intensity) / 6f, GetSin(_degrees, _intensity) / 2f);
        //    //use coords for speed of movement
        //    //add sophistications like extra blame during it and perhaps convulsions
        //}
        private static Vector2 MankoCircle(float deg, float step, float intensity, out float newDeg)
        {
            // 14.
            deg = Mathf.Repeat(deg, 360);
            newDeg = deg += step * intensity;

            //return new Vector2(DividePos(GetCos(deg, intensity), 2f), DividePos(GetSin(deg, intensity), 2f));
            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity * 2f));
        }
        private static Vector2 MankoHorizontalOval(float deg, float step, float intensity, out float newDeg)
        {
            // 15.
            deg = Mathf.Repeat(deg, 360);
            newDeg = deg += step * intensity;

            //return new Vector2(GetCos(deg, intensity), DividePos(GetSin(deg, intensity), 4f));
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 4f));
        }
        private static Vector2 MankoSideToSide(float deg, float step, float intensity, out float newDeg)
        {
            // 16.
            deg = Mathf.Repeat(deg, 360);
            newDeg = deg += step * (intensity * 2f);

            //return new Vector2(GetCos(deg, intensity), DividePos(GetSin(deg, intensity), 4f));
            return new Vector2(GetCos(deg, intensity), GetSin(deg, intensity * 4f));
        }
        private static Vector2 MankoSideToSideFast(float deg, float step, float intensity, out float newDeg)
        {
            // 17.
            deg = Mathf.Repeat(deg, 360);
            newDeg = deg += step * (intensity * 4f);

            return new Vector2(GetCos(deg, intensity * 2f), GetSin(deg, intensity * 4f));
        }
    }
}
