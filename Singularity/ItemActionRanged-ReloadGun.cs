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
		public static void Postfix_ReloadGun(ItemActionRanged __instance, ItemActionData _actionData)
		{
			if (!(_actionData is ItemActionRanged.ItemActionDataRanged actionDataRanged))
				return;

			if (actionDataRanged.invData.holdingEntity.isEntityRemote)
				return;

			UpdateSounds(__instance, actionDataRanged);
		}
	}
}
