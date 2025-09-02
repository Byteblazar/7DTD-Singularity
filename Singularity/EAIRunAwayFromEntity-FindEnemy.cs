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

public abstract partial class EAIRunawayFromEntity_Patches
{
	public static bool Prefix_FindEnemy(EAIRunawayFromEntity __instance)
	{
		__instance.avoidEntity = null;
		if ((bool)__instance.theEntity.noisePlayer
			&& __instance.theEntity.noisePlayerVolume >= EAIRunawayFromEntity.cRunNoiseVolume
			&& __instance.targetClasses?.Contains(typeof(EntityPlayer)) == true)
		{
			__instance.avoidEntity = __instance.theEntity.noisePlayer;
		}
		else
		{
			float seeDistance1 = __instance.theEntity.GetSeeDistance();
			Bounds _bb = BoundsUtils.ExpandBounds(__instance.theEntity.boundingBox, seeDistance1, seeDistance1, seeDistance1);
			foreach (var targetClass in __instance.targetClasses)
			{
				__instance.theEntity.world.GetEntitiesInBounds(targetClass, _bb, EAIRunawayFromEntity.list);
				if (targetClass == typeof(EntityPlayer))
				{
					float num = float.MaxValue;
					foreach (var entity in EAIRunawayFromEntity.list)
					{
						EntityPlayer? entityPlayer = entity as EntityPlayer;
						float seeDistance2 = __instance.manager.GetSeeDistance(entityPlayer);
						if ((double)seeDistance2 < (double)num && __instance.theEntity.CanSee(entityPlayer) && __instance.theEntity.CanSeeStealth(seeDistance2, entityPlayer.Stealth.lightLevel) && !entityPlayer.IsIgnoredByAI())
						{
							num = seeDistance2;
							__instance.avoidEntity = entityPlayer;
						}
					}
				}
				else
				{
					float num = float.MaxValue;
					for (int index3 = 0; index3 < EAIRunawayFromEntity.list.Count; ++index3)
					{
						EntityAlive? _other = EAIRunawayFromEntity.list[index3] as EntityAlive;
						float distanceSq = __instance.theEntity.GetDistanceSq(_other);
						if ((double)distanceSq <= __instance.minSneakDistance * (double)__instance.minSneakDistance)
						{
							__instance.avoidEntity = _other;
							break;
						}
						if ((double)distanceSq < (double)num && __instance.theEntity.CanSee(_other) && !_other.IsIgnoredByAI())
						{
							num = distanceSq;
							__instance.avoidEntity = _other;
						}
					}
				}
				EAIRunawayFromEntity.list.Clear();
				if ((bool)__instance.avoidEntity)
					break;
			}
		}
		return false;
	}
}
