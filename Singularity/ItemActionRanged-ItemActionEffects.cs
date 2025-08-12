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
    public abstract partial class ItemActionRanged_Patches
    {
        public static void Prefix_ItemActionEffects(ItemActionRanged __instance,
                           GameManager _gameManager,
                           ItemActionData _actionData,
                           int _firingState,
                           Vector3 _startPos,
                           Vector3 _direction,
                           int _userData = 0)
        {
            var localPlayer = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (!Utils.IsMultiplayerSession && _actionData.invData?.holdingEntity?.entityId != localPlayer?.entityId)
                return;

            if (_actionData.invData?.itemValue == null) return;
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

            string? ammoSoundStart = null;
            string? ammoSoundLoop = null;

            ammoClass.Properties?.Values.TryGetValue("Singularity_Sound_start", out ammoSoundStart);
            ammoClass.Properties?.Values.TryGetValue("Singularity_Sound_loop", out ammoSoundLoop);

            DynamicProperties props = _actionData.invData.itemValue.ItemClass.Actions[0].Properties;

            if (string.IsNullOrEmpty(ammoSoundStart))
                props.Values.TryGetValue("Sound_start", out ammoSoundStart);

            if (string.IsNullOrEmpty(ammoSoundLoop))
                props.Values.TryGetValue("Sound_loop", out ammoSoundLoop);

            actionDataRanged.SoundStart = ammoSoundStart;
            actionDataRanged.SoundLoop = ammoSoundLoop;
        }
    }
}
