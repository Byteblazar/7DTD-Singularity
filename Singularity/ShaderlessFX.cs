/*
 * Singularity
 * Copyright © 2025 Byteblazar <byteblazar@protonmail.com> * 
 * 
 * 
 * This file is part of Singularity.
 * 
 * Singularity is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Singularity is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Singularity. If not, see <https://www.gnu.org/licenses/>. 
 * 
*/

using System.Collections;
using UnityEngine;

namespace Singularity
{
    public class ShaderlessFX : MonoBehaviour
    {
        // ---------- Snapshot / burn ----------
        Texture2D? _snapshot;
        bool _burning;
        float _burnAlpha = 0f;
        float _burnTargetAlpha = 0f;
        float _burnSpeed = 0f;

        // Capture control (schedule a single capture at end-of-frame)
        bool _pendingCapture = false;
        bool _captureCoroutineRunning = false;

        // ---------- OverlayRGB ----------
        public Color OverlayColor { get; set; } = Color.white;
        float _overlayAlpha = 0f;
        float _overlayTargetAlpha = 0f;
        float _overlaySpeed = 0f;

        void OnGUI()
        {
            if (_snapshot != null && _burnAlpha > 0f)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, _burnAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _snapshot, ScaleMode.StretchToFill);
                GUI.color = prev;
            }

            float overlayFinal = _overlayAlpha * OverlayColor.a;
            if (overlayFinal > 0f)
            {
                var prev = GUI.color;
                GUI.color = new Color(OverlayColor.r, OverlayColor.g, OverlayColor.b, overlayFinal);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = prev;
            }
        }

        void Update()
        {
            if (_overlaySpeed > 0f)
            {
                _overlayAlpha = Mathf.MoveTowards(_overlayAlpha, _overlayTargetAlpha, _overlaySpeed * Time.deltaTime);
                if (Mathf.Approximately(_overlayAlpha, _overlayTargetAlpha))
                    _overlaySpeed = 0f;
            }

            if (_burnSpeed > 0f)
            {
                _burnAlpha = Mathf.MoveTowards(_burnAlpha, _burnTargetAlpha, _burnSpeed * Time.deltaTime);
                if (Mathf.Approximately(_burnAlpha, _burnTargetAlpha))
                    _burnSpeed = 0f;
            }

            if (_burning && _burnAlpha <= 0f)
            {
                _burning = false;
                if (_snapshot != null)
                {
                    Destroy(_snapshot);
                    _snapshot = null;
                }
            }
        }

        public void Snapshot(float intensity, float fade)
        {
            float target = Mathf.Clamp01(intensity);

            if (fade <= 0f)
            {
                _burnAlpha = target;
                _burnTargetAlpha = target;
                _burnSpeed = 0f;
            }
            else
            {
                _burnTargetAlpha = target;
                _burnSpeed = Mathf.Abs(_burnTargetAlpha - _burnAlpha) / Mathf.Max(0.0001f, fade);
            }

            if (target > 0f)
            {
                if (!_pendingCapture)
                {
                    _pendingCapture = true;
                    if (!_captureCoroutineRunning)
                        StartCoroutine(CaptureAtEndOfFrame());
                }
            }

            _burning = (_burnAlpha > 0f) || (_burnTargetAlpha > 0f) || _pendingCapture;
        }

        IEnumerator CaptureAtEndOfFrame()
        {
            _captureCoroutineRunning = true;
            yield return new WaitForEndOfFrame();

            _pendingCapture = false;

            int w = Screen.width, h = Screen.height;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            if (_snapshot != null) Destroy(_snapshot);
            _snapshot = tex;

            _burning = true;

            _captureCoroutineRunning = false;
        }

        public void OverlayRGB(float intensity, float duration)
        {
            float target = Mathf.Clamp01(intensity);
            if (duration <= 0f)
            {
                _overlayAlpha = target;
                _overlayTargetAlpha = target;
                _overlaySpeed = 0f;
                return;
            }
            _overlayTargetAlpha = target;
            _overlaySpeed = Mathf.Abs(_overlayTargetAlpha - _overlayAlpha) / Mathf.Max(0.0001f, duration);
        }

        public void OverlayRGB(Color color, float intensity, float duration)
        {
            OverlayColor = color;
            OverlayRGB(intensity, duration);
        }

        public void OverlayRGB(string rgbaString, float intensity, float duration)
        {
            OverlayColor = Utils.ParseAnyColor(rgbaString);
            OverlayRGB(intensity, duration);
        }

        public void SetOverlayColor(string c)
        {
            OverlayColor = Utils.ParseAnyColor(c);
        }
    }
}

