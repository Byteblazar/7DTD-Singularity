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

namespace Singularity.DynamicPatches;

public abstract partial class EAIWander_Patches
{
	/*
	 * am I a loner?
	 *		yes -> return
	 *		no -> am I an alpha?
	 *				yes -> look for entities of my type around (short-circuit)
	 *						there's at least one -> I remain an alpha -> return true
	 *						there's none -> I'm no longer an alpha -> return true
	 *			
	 *				no -> do I already follow an alpha?
	 *						yes -> is it alive?
	 *								yes -> if distance > max, go to them -> return false
	 *								no -> I don't follow an alpha anymore (jump to the no-alpha case)
	 *						no -> is there an alpha around? (short-circuit)
	 *								yes -> go to them & remember them for later
	 *								no -> are there other entities of my type around (except loners) (short-circuit)?
	 *										yes -> I'm the alpha now
	 *										no -> do nothing
	 */

	public static bool Prefix_Start(EAIWander __instance)
	{
		if (__instance.theEntity?.IsAlive() != true) return true;
		var entity = __instance.theEntity;
		Gregariousness.Data gData = Gregariousness.GetOrCreate(entity);
		if (gData.IsSolitary) return true;

		if (gData.GetInAlphaCooldown()) return true;

		Gregariousness.cachedEntities.Clear();

		if (gData.IsAlpha)
		{
			if (Gregariousness.GetMaxAllies(entity)) return true;
			gData.IsAlpha = false;
			return true;
		}

		if (gData.MyAlpha != null)
		{
			if (gData.MyAlpha.IsAlive())
			{
				if (gData.MyAlpha.GetDistanceSq(entity) > Gregariousness.assistRadius / 2 * Gregariousness.assistRadius / 2)
				{
					entity.FindPath(gData.MyAlpha.position, entity.GetMoveSpeedAggro(), false, __instance);
					return false;
				}
			}
			else gData.MyAlpha = null;
		}

		if (gData.MyAlpha == null)
		{
			if (Gregariousness.TryFindAlpha(entity, out var alpha))
			{
				gData.MyAlpha = alpha;
				entity.FindPath(alpha.position, entity.GetMoveSpeedAggro(), false, __instance);
				return false;
			}
			else if (Gregariousness.AreThereAlliesAround(entity))
			{
				gData.IsAlpha = true;
				Log.Warning($"{entity.EntityClass.classname} set to Alpha. Pos: {entity.position}");
			}
		}

		return true;
	}
}
