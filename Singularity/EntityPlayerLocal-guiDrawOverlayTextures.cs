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

using UnityEngine;

namespace Singularity
{
    public abstract partial class EntityPlayerLocal_Patches
    {
        public static bool UseCustomDrowningEffects { get; set; } = false;
        public static bool UseCustomBurningEffects { get; set; } = false;
        public static bool DisableBloodDropsOverlay { get; set; } = false;
        public static float Timer { get; set; } = 0f;
        public static float DrowningPower { get; set; } = 0f;
        public static bool Burning { get; set; } = false;

        public static void Prefix_guiDrawOverlayTextures(EntityPlayerLocal __instance)
        {
            EntityPlayerLocal p = __instance;
            if (Burning || DrowningPower > 0f)
            {
                p.healthLostThisRound = false;
                Timer += Time.unscaledDeltaTime;
                if (Timer < 1f || DeathCam) return;
                Timer = 0f;
            }

            ScreenEffects se = p.ScreenEffectManager;

            if (UseCustomDrowningEffects)
            {
                if (p.Buffs.HasBuff("buffDrowning03"))
                {
                    p.healthLostThisRound = false;
                    DrowningPower = 1f - p.Stats.Health.ValuePercent;
                    se.SetScreenEffect("FadeToBlack", DrowningPower, 1f);
                    se.SetScreenEffect("Blur", DrowningPower, 1f);
                    se.SetScreenEffect("Dark", DrowningPower, 1f);
                    se.SetScreenEffect("Drunk", DrowningPower / 2, 1f);
                    Singularity.fx.OverlayColor = Color.black;
                    se.SetScreenEffect("Singularity_Overlay", .1f, 0f);
                    se.SetScreenEffect("Singularity_Overlay", 0f, 1f);
                    se.SetScreenEffect("Singularity_Snapshot", 1f, 0f);
                    se.SetScreenEffect("Singularity_Snapshot", 0f, .2f);
                    return;
                }
                else if (DrowningPower > 0f && p.Stats.Health.ValuePercent > .5f)
                {
                    DrowningPower = 0f;
                    se.SetScreenEffect("FadeToBlack", 0f, 1f);
                    se.SetScreenEffect("Blur", 0f, 1f);
                    se.SetScreenEffect("Dark", 0f, 1f);
                    se.SetScreenEffect("Drunk", 0f, 1f);
                    return;
                }
            }

            if (UseCustomBurningEffects)
            {
                if (p.Buffs.HasBuff("buffIsOnFire"))
                {
                    p.healthLostThisRound = false;
                    Burning = true;
                    se.SetScreenEffect("Hot3", 1f, 1f);
                    se.SetScreenEffect("Dark", 1f, 1f);
                    Singularity.fx.OverlayColor = Color.red;
                    se.SetScreenEffect("Singularity_Overlay", .1f, 0f);
                    se.SetScreenEffect("Singularity_Overlay", 0f, 1f);
                    se.SetScreenEffect("Singularity_Snapshot", 1f, 0f);
                    se.SetScreenEffect("Singularity_Snapshot", 0f, .2f);
                    //se.SetScreenEffect("FeralVision", 0.1f, 1f);
                    return;
                }
                else if (Burning)
                {
                    Burning = false;
                    se.SetScreenEffect("Hot3", 0f, 1f);
                    se.SetScreenEffect("Dark", 0f, 1f);
                    //se.SetScreenEffect("FeralVision", 0f, 1f);
                }
            }

            if (DisableBloodDropsOverlay)
                p.healthLostThisRound = false;
        }
    }
}
