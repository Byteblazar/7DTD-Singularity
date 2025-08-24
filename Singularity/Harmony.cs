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

using HarmonyLib;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace Singularity
{
	public class Singularity : IModApi
	{
		public static Harmony? harmony;
		public static GameObject? go;
		public static ShaderlessFX? fx;
		public static bool Patched = false;
		public void InitMod(Mod _modInstance)
		{
			harmony = new Harmony(Assembly.GetExecutingAssembly().FullName);

			ModEvents.MainMenuOpened.RegisterHandler((ref ModEvents.SMainMenuOpenedData data) =>
			{
				harmony?.UnpatchSelf();
				Patched = false;
			});

			ModEvents.PlayerSpawnedInWorld.RegisterHandler((ref ModEvents.SPlayerSpawnedInWorldData data) =>
			{
				if (Patched) return;
				Patched = true;

				//harmony.PatchAll(Assembly.GetExecutingAssembly());
				if (Config.CheckFeatureStatus("XMLExtensions", "ShaderlessFX"))
				{
					InitShaderlessFX();
					var original = AccessTools.Method(typeof(ScreenEffects), nameof(ScreenEffects.SetScreenEffect), new Type[] { typeof(string), typeof(float), typeof(float) });
					var postfix = AccessTools.Method(typeof(ScreenEffects_Patches), nameof(ScreenEffects_Patches.Postfix_SetScreenEffect));
					harmony?.Patch(
						original,
						postfix: new HarmonyMethod(postfix));

					original = AccessTools.Method(typeof(MinEventActionModifyScreenEffect), nameof(MinEventActionModifyScreenEffect.ParseXmlAttribute), new Type[] { typeof(XAttribute) });
					var prefix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Prefix_ParseXmlAttribute));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));

					original = AccessTools.Method(typeof(MinEventActionModifyScreenEffect), nameof(MinEventActionModifyScreenEffect.Execute), new Type[] { typeof(MinEventParams) });
					prefix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Prefix_Execute));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));
				}

				if (Config.CheckFeatureStatus("XMLExtensions", "EntityStun"))
				{
					var original = AccessTools.Method(typeof(EntityLookHelper), nameof(EntityLookHelper.onUpdateLook));
					var prefix = AccessTools.Method(typeof(EntityLookHelper_Patches), nameof(EntityLookHelper_Patches.Prefix_onUpdateLook));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));

					original = AccessTools.Method(typeof(EntityMoveHelper), nameof(EntityMoveHelper.UpdateMoveHelper));
					prefix = AccessTools.Method(typeof(EntityMoveHelper_Patches), nameof(EntityMoveHelper_Patches.Prefix_UpdateMoveHelper));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));

					original = AccessTools.Method(typeof(EModelBase), nameof(EModelBase.LookAtUpdate), new Type[] { typeof(EntityAlive) });
					prefix = AccessTools.Method(typeof(EModelBase_Patches), nameof(EModelBase_Patches.Prefix_LookAtUpdate));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));
				}

				if (Config.CheckFeatureStatus("XMLExtensions", "NonlethalExplosives"))
				{
					var original = AccessTools.Method(typeof(Explosion), nameof(Explosion.AttackEntites), new Type[] { typeof(int), typeof(ItemValue), typeof(EnumDamageTypes) });
					var prefix = AccessTools.Method(typeof(Explosion_Patches), nameof(Explosion_Patches.Prefix_AttackEntities));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(prefix));
				}

				if (Config.CheckFeatureStatus("XMLExtensions", "AmmoSounds"))
				{
					var original = AccessTools.Method(typeof(ItemActionRanged), nameof(ItemActionRanged.OnModificationsChanged), new Type[] { typeof(ItemActionData) });
					var postfix = AccessTools.Method(typeof(ItemActionRanged_Patches), nameof(ItemActionRanged_Patches.Postfix_OnModificationsChanged));
					harmony?.Patch(
						original,
						postfix: new HarmonyMethod(postfix));

					original = AccessTools.Method(typeof(ItemActionRanged), nameof(ItemActionRanged.SwapAmmoType), new Type[] { typeof(EntityAlive), typeof(int) });
					postfix = AccessTools.Method(typeof(ItemActionRanged_Patches), nameof(ItemActionRanged_Patches.Postfix_SwapAmmoType));
					harmony?.Patch(
						original,
						prefix: new HarmonyMethod(postfix));
				}

				if (!GameManager.IsDedicatedServer)
				{
					EntityPlayerLocal_Patches.UseCustomBurningEffects = Config.CheckFeatureStatus("ScreenEffects", "UseCustomBurningEffects");
					EntityPlayerLocal_Patches.UseCustomDrowningEffects = Config.CheckFeatureStatus("ScreenEffects", "UseCustomDrowningEffects");
					EntityPlayerLocal_Patches.DisableBloodDropsOverlay = Config.CheckFeatureStatus("ScreenEffects", "DisableBloodDropsOverlay");

					if (
						EntityPlayerLocal_Patches.UseCustomBurningEffects
						|| EntityPlayerLocal_Patches.UseCustomDrowningEffects
						|| EntityPlayerLocal_Patches.DisableBloodDropsOverlay
					)
					{
						InitShaderlessFX();
						EntityPlayerLocal_Patches.Burning = false;
						EntityPlayerLocal_Patches.DrowningPower = 0f;
						var original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.guiDrawOverlayTextures), new Type[] { typeof(NGuiWdwInGameHUD), typeof(bool) });
						var prefix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Prefix_guiDrawOverlayTextures));
						harmony?.Patch(
							original,
							prefix: new HarmonyMethod(prefix));

						original = AccessTools.Method(typeof(ScreenEffects), nameof(ScreenEffects.SetScreenEffect), new Type[] { typeof(string), typeof(float), typeof(float) });
						prefix = AccessTools.Method(typeof(ScreenEffects_Patches), nameof(ScreenEffects_Patches.Prefix_SetScreenEffect));
						harmony?.Patch(
							original,
							prefix: new HarmonyMethod(prefix));

						original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.startDeathCamera));
						prefix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Prefix_startDeathCamera));
						harmony?.Patch(
							original,
							prefix: new HarmonyMethod(prefix));

						original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.startDeathCamera));
						var postfix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Postfix_startDeathCamera));
						harmony?.Patch(
							original,
							postfix: new HarmonyMethod(postfix));
					}
				}
			});
		}
		public static void InitShaderlessFX()
		{
			if (GameManager.IsDedicatedServer) return;

			var cam = GameManager.Instance?.myEntityPlayerLocal?.m_vp_FPCamera;
			if (cam != null && cam.gameObject.GetComponent<ShaderlessFX>() == null)
			{
				go = new GameObject("ShaderlessFX");
				go.transform.SetParent(cam.transform, false);
				fx = go.AddComponent<ShaderlessFX>();
				go.SetActive(true);
			}
		}
	}
}
