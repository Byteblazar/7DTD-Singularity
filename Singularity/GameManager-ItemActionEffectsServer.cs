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
    public abstract partial class GameManager_Patches
    {
        public static void Prefix_ItemActionEffectsServer(
                           GameManager __instance,
                           int _entityId,
                           int _slotIdx,
                           int _itemActionIdx,
                           int _firingState,
                           Vector3 _startPos,
                           Vector3 _direction,
                           int _userData = 0)
        {
            if (__instance.m_World.IsRemote()) return;

            int _allButAttachedToEntityId = _entityId;
            EntityPlayer? entity = __instance.m_World.GetEntity(_entityId) as EntityPlayer;
            if (entity == null) return;
            if (entity.AttachedMainEntity != null)
                _allButAttachedToEntityId = entity.AttachedMainEntity.entityId;

            var action = entity.inventory.GetItemActionInSlot(_slotIdx, _itemActionIdx) as ItemActionRanged;
            var actionData = entity.inventory.GetItemActionDataInSlot(_slotIdx, _itemActionIdx) as ItemActionRanged.ItemActionDataRanged;
            if (action == null || actionData == null) return;

            int ammoItemId = 0;

            string[] magNames = action.MagazineItemNames;
            if (magNames != null && magNames.Length > 0)
            {
                int selIndex = actionData.invData.itemValue.SelectedAmmoTypeIndex;
                if (selIndex < 0 || selIndex >= magNames.Length) selIndex = 0;
                string ammoName = magNames[selIndex];
                if (!string.IsNullOrEmpty(ammoName))
                {
                    ItemClass ammoClass = ItemClass.GetItemClass(ammoName);
                    if (ammoClass != null) ammoItemId = ammoClass.Id;
                }
            }

            if (ammoItemId == 0) return;

            var pkg = NetPackageManager.GetPackage<NetPackageItemActionSound>().Setup(_entityId, _slotIdx, _itemActionIdx, ammoItemId);
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(pkg,
                _allButAttachedToEntityId: _allButAttachedToEntityId,
                _entitiesInRangeOfEntity: _entityId);
        }
    }
}
