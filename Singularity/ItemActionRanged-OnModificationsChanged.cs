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

namespace Singularity
{
	public abstract partial class ItemActionRanged_Patches
	{
		public static void Postfix_OnModificationsChanged(ItemActionRanged __instance, ItemActionData _data)
		{
			UpdateSounds(__instance, _data);
		}

		public static void UpdateSounds(ItemActionRanged __instance, ItemActionData? _actionData)
		{
			// runs locally in clients and for the host in multiplayer sessions
			var localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (!Utils.IsMultiplayerHost && _actionData?.invData?.holdingEntity?.entityId != localPlayer?.entityId)
				return;

			// this method needs to know if the weapon is overriding sounds (item modifiers), and if not, it applies custom ammo sounds as needed
			// then it informs the server

			if (_actionData?.invData?.itemValue == null) return;
			var actionDataRanged = _actionData as ItemActionRanged.ItemActionDataRanged;
			if (actionDataRanged == null) return;

			int selIndex = _actionData.invData.itemValue.SelectedAmmoTypeIndex;

			string[] magNames = __instance.MagazineItemNames;
			if (magNames == null || magNames.Length == 0) return;
			if (selIndex < 0 || selIndex >= magNames.Length) selIndex = 0;

			string ammoName = magNames[selIndex];
			if (string.IsNullOrEmpty(ammoName)) return;

			ItemClass ammoClass = ItemClass.GetItemClass(ammoName);
			if (ammoClass == null) return;

			ItemValue itemValue = actionDataRanged.invData.itemValue;
			DynamicProperties props = _actionData.invData.itemValue.ItemClass.Actions[0].Properties;
			props.Values.TryGetValue("Sound_start", out actionDataRanged.SoundStart);
			props.Values.TryGetValue("Sound_loop", out actionDataRanged.SoundLoop);

			if (ammoClass.Properties.Values.TryGetValue("Singularity_Sound_start", out var ammoSoundStart))
				actionDataRanged.SoundStart = ammoSoundStart;

			if (ammoClass.Properties.Values.TryGetValue("Singularity_Sound_loop", out var ammoSoundLoop))
				actionDataRanged.SoundLoop = ammoSoundLoop;

			actionDataRanged.SoundStart = itemValue.GetPropertyOverride("Sound_start", actionDataRanged.SoundStart);
			actionDataRanged.SoundLoop = itemValue.GetPropertyOverride("Sound_loop", actionDataRanged.SoundLoop);

			int slotIdx = actionDataRanged.invData.slotIdx;
			int actionIdx = actionDataRanged.indexInEntityOfAction;

			var pkg = NetPackageManager.GetPackage<NetPackageItemActionSound>().Setup(
					localPlayer.entityId,
					slotIdx,
					actionIdx,
					actionDataRanged.SoundStart,
					actionDataRanged.SoundLoop
				);

			if (GameManager.Instance.World.IsRemote())
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(pkg);
			else SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(pkg, _allButAttachedToEntityId: localPlayer.entityId);
		}
	}
}
