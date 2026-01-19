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
using Singularity.DynamicPatches;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

namespace Singularity;

[Harmony]
public class StaticPatches
{
	public static Harmony DynamicHarmony { get; private set; } = new Harmony("com.byteblazar.singularity.dynamic");
	public static bool Patched = false;
	public static bool? AllowSpawnNearBackpack;

	[HarmonyFinalizer]
	[HarmonyPatch(typeof(XUiC_MainMenu), nameof(XUiC_MainMenu.OnOpen))]
	public static void Finalizer_OnOpen()
	{
		DynamicHarmony?.UnpatchSelf();
		if (AllowSpawnNearBackpack != null)
			GamePrefs.Set(EnumGamePrefs.AllowSpawnNearBackpack, (bool)AllowSpawnNearBackpack);

		Patched = false;
	}

	[HarmonyFinalizer]
	[HarmonyPatch(typeof(GameManager), nameof(GameManager.PlayerSpawnedInWorld))]
	public static void Finalizer_PlayerSpawnedInWorld()
	{
		if (Patched) return;
		Patched = true;

		if (Config.CheckFeatureStatus("XMLExtensions", "ShaderlessFX"))
		{
			Singularity.InitShaderlessFX();
			var original = AccessTools.Method(typeof(ScreenEffects), nameof(ScreenEffects.SetScreenEffect), new Type[] { typeof(string), typeof(float), typeof(float) });
			var postfix = AccessTools.Method(typeof(ScreenEffects_Patches), nameof(ScreenEffects_Patches.Postfix_SetScreenEffect));
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

			original = AccessTools.Method(typeof(MinEventActionModifyScreenEffect), nameof(MinEventActionModifyScreenEffect.ParseXmlAttribute), new Type[] { typeof(XAttribute) });
			var prefix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Prefix_ParseXmlAttribute));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

			original = AccessTools.Method(typeof(MinEventActionModifyScreenEffect), nameof(MinEventActionModifyScreenEffect.Execute), new Type[] { typeof(MinEventParams) });
			prefix = AccessTools.Method(typeof(MinEventActionModifyScreenEffect_Patches), nameof(MinEventActionModifyScreenEffect_Patches.Prefix_Execute));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "EntityStun"))
		{
			var original = AccessTools.Method(typeof(EntityLookHelper), nameof(EntityLookHelper.onUpdateLook));
			var prefix = AccessTools.Method(typeof(EntityLookHelper_Patches), nameof(EntityLookHelper_Patches.Prefix_onUpdateLook));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

			original = AccessTools.Method(typeof(EntityMoveHelper), nameof(EntityMoveHelper.UpdateMoveHelper));
			prefix = AccessTools.Method(typeof(EntityMoveHelper_Patches), nameof(EntityMoveHelper_Patches.Prefix_UpdateMoveHelper));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

			original = AccessTools.Method(typeof(EModelBase), nameof(EModelBase.LookAtUpdate), new Type[] { typeof(EntityAlive) });
			prefix = AccessTools.Method(typeof(EModelBase_Patches), nameof(EModelBase_Patches.Prefix_LookAtUpdate));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "NonlethalExplosives"))
		{
			var original = AccessTools.Method(typeof(Explosion), nameof(Explosion.AttackEntites), new Type[] { typeof(int), typeof(ItemValue), typeof(EnumDamageTypes) });
			var prefix = AccessTools.Method(typeof(Explosion_Patches), nameof(Explosion_Patches.Prefix_AttackEntities));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "AmmoSounds"))
		{
			var original = AccessTools.Method(typeof(ItemActionRanged), nameof(ItemActionRanged.OnModificationsChanged), new Type[] { typeof(ItemActionData) });
			var postfix = AccessTools.Method(typeof(ItemActionRanged_Patches), nameof(ItemActionRanged_Patches.Postfix_OnModificationsChanged));
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

			original = AccessTools.Method(typeof(ItemActionRanged), nameof(ItemActionRanged.ReloadGun), new Type[] { typeof(ItemActionData) });
			postfix = AccessTools.Method(typeof(ItemActionRanged_Patches), nameof(ItemActionRanged_Patches.Postfix_ReloadGun));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(postfix));
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "EntityGregariousness"))
		{
			var original = AccessTools.Method(typeof(EntityAlive), nameof(EntityAlive.SetAttackTarget), new Type[] { typeof(EntityAlive), typeof(int) });
			var postfix = AccessTools.Method(typeof(EntityAlive_Patches), nameof(EntityAlive_Patches.Postfix_SetAttackTarget));
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

			if (Utils.IsOfflineSingleplayer)
			{
				original = AccessTools.Method(typeof(EntityAlive), nameof(EntityAlive.SetAttackTargetClient), new Type[] { typeof(EntityAlive) });
				DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));
			}

			original = AccessTools.Method(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponse), new Type[] { typeof(DamageResponse) });
			postfix = AccessTools.Method(typeof(EntityAlive_Patches), nameof(EntityAlive_Patches.Postfix_ProcessDamageResponse));
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));
			original = AccessTools.Method(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponseLocal), new Type[] { typeof(DamageResponse) });
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

			original = AccessTools.Method(typeof(EAIWander), nameof(EAIWander.Start));
			var prefix = AccessTools.Method(typeof(EAIWander_Patches), nameof(EAIWander_Patches.Prefix_Start));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix) { priority = Priority.Last });
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "SetAsTargetIfHurt_BypassSameTypeCheck"))
		{
			var original = AccessTools.Method(typeof(EAISetAsTargetIfHurt), nameof(EAISetAsTargetIfHurt.CanExecute));
			var transpiler = AccessTools.Method(typeof(EAISetAsTargetIfHurt_Patches), nameof(EAISetAsTargetIfHurt_Patches.Transpiler_CanExecute));
			DynamicHarmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "RunawayFromEntity_NoImplicitPlayer"))
		{
			var original = AccessTools.Method(typeof(EAIRunawayFromEntity), nameof(EAIRunawayFromEntity.FindEnemy));
			var prefix = AccessTools.Method(typeof(EAIRunawayFromEntity_Patches), nameof(EAIRunawayFromEntity_Patches.Prefix_FindEnemy));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix) { priority = Priority.Last });
		}

		if (Config.CheckFeatureStatus("XMLExtensions", "SetNearestCorpseAsTarget_Filters"))
		{
			var original = AccessTools.Method(typeof(EAISetNearestCorpseAsTarget), nameof(EAISetNearestCorpseAsTarget.SetData), new Type[] { typeof(DictionarySave<string, string>) });
			var postfix = AccessTools.Method(typeof(EAISetNearestCorpseAsTarget_Patches), nameof(EAISetNearestCorpseAsTarget_Patches.Postfix_SetData));
			DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

			original = AccessTools.Method(typeof(EAISetNearestCorpseAsTarget), nameof(EAISetNearestCorpseAsTarget.CanExecute));
			var prefix = AccessTools.Method(typeof(EAISetNearestCorpseAsTarget_Patches), nameof(EAISetNearestCorpseAsTarget_Patches.Prefix_CanExecute));
			DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix) { priority = Priority.Last });
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
				Singularity.InitShaderlessFX();
				EntityPlayerLocal_Patches.Burning = false;
				EntityPlayerLocal_Patches.DrowningPower = 0f;
				var original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.guiDrawOverlayTextures), new Type[] { typeof(NGuiWdwInGameHUD), typeof(bool) });
				var prefix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Prefix_guiDrawOverlayTextures));
				DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(ScreenEffects), nameof(ScreenEffects.SetScreenEffect), new Type[] { typeof(string), typeof(float), typeof(float) });
				prefix = AccessTools.Method(typeof(ScreenEffects_Patches), nameof(ScreenEffects_Patches.Prefix_SetScreenEffect));
				DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.startDeathCamera));
				prefix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Prefix_startDeathCamera));
				DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.startDeathCamera));
				var postfix = AccessTools.Method(typeof(EntityPlayerLocal_Patches), nameof(EntityPlayerLocal_Patches.Postfix_startDeathCamera));
				DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));
			}

			if (Config.CheckFeatureStatus("ServerPolicies", "EnforceBrightness"))
			{
				XUiC_OptionsVideo_Patches.Target = float.Parse(Config.GetPropertyValue("ServerPolicies", "TargetBrightness"));

				var original = AccessTools.Method(typeof(XUiC_OptionsVideo), nameof(XUiC_OptionsVideo.Init));
				var finalizer = AccessTools.Method(typeof(XUiC_OptionsVideo_Patches), nameof(XUiC_OptionsVideo_Patches.Finalizer_Init));
				DynamicHarmony.Patch(original, finalizer: new HarmonyMethod(finalizer));

				original = AccessTools.Method(typeof(XUiC_OptionsVideo), nameof(XUiC_OptionsVideo.OnOpen));
				DynamicHarmony.Patch(original, finalizer: new HarmonyMethod(finalizer));

				original = AccessTools.Method(typeof(XUiC_OptionsVideoSimplified), nameof(XUiC_OptionsVideoSimplified.Init));
				finalizer = AccessTools.Method(typeof(XUiC_OptionsVideoSimplified_Patches), nameof(XUiC_OptionsVideoSimplified_Patches.Finalizer_Init));
				DynamicHarmony.Patch(original, finalizer: new HarmonyMethod(finalizer));

				original = AccessTools.Method(typeof(XUiC_OptionsVideoSimplified), nameof(XUiC_OptionsVideoSimplified.OnOpen));
				DynamicHarmony.Patch(original, finalizer: new HarmonyMethod(finalizer));

				original = AccessTools.Method(typeof(XUiC_OptionsVideoSimplified), nameof(XUiC_OptionsVideoSimplified.Init));
				finalizer = AccessTools.Method(typeof(XUiC_OptionsVideoSimplified_Patches), nameof(XUiC_OptionsVideoSimplified_Patches.Finalizer_Init));
				DynamicHarmony.Patch(original, finalizer: new HarmonyMethod(finalizer));

				GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, XUiC_OptionsVideo_Patches.Target);
				GamePrefs.Instance?.Save();
				GameOptionsManager.ApplyAllOptions(LocalPlayerUI.primaryUI);
			}
			if (Config.CheckFeatureStatus("ServerPolicies", "NoRespawnNearBackpack"))
			{
				var original = AccessTools.Method(typeof(XUiC_SpawnSelectionWindow), nameof(XUiC_SpawnSelectionWindow.SpawnButtonPressed), new Type[] { typeof(SpawnMethod), typeof(int) });
				var prefix = AccessTools.Method(typeof(XUiC_SpawnSelectionWindow_Patches), nameof(XUiC_SpawnSelectionWindow_Patches.Prefix_SpawnButtonPressed));
				DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(GameManager), nameof(GameManager.Cleanup));
				prefix = AccessTools.Method(typeof(GameManager_Patches), nameof(GameManager_Patches.Prefix_Cleanup));
				DynamicHarmony.Patch(original, prefix: new HarmonyMethod(prefix));

				original = AccessTools.Method(typeof(GameServerInfo), nameof(GameServerInfo.GetValue), new Type[] { typeof(GameInfoBool) });
				var postfix = AccessTools.Method(typeof(GameServerInfo_Patches), nameof(GameServerInfo_Patches.Postfix_GetValue));
				DynamicHarmony.Patch(original, postfix: new HarmonyMethod(postfix));

				AllowSpawnNearBackpack ??= GamePrefs.GetBool(EnumGamePrefs.AllowSpawnNearBackpack);
				GamePrefs.Set(EnumGamePrefs.AllowSpawnNearBackpack, false);
			}
		}
	}
}

public class Singularity : IModApi
{
	public static Harmony StaticHarmony { get; private set; } = new Harmony("com.byteblazar.singularity.static");
	public static GameObject? go;
	public static ShaderlessFX? fx;

	//public static Dictionary<string, PatchInfo> patches = new();
	//public class PatchInfo { public MethodBase? original; public MethodInfo? patch; }

	public void InitMod(Mod _modInstance)
	{
		StaticHarmony.PatchAll(Assembly.GetExecutingAssembly());
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
