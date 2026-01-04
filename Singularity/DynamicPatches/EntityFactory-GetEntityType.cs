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

namespace Singularity.DynamicPatches;

[HarmonyPatch(typeof(EntityFactory))]
public abstract partial class EntityFactory_Patches
{
	public static Dictionary<string, Type> EntityTypes = new()
	{
	   { nameof(EntityAnimalSupernatural), typeof(EntityAnimalSupernatural) },
	   { nameof(EntityAnimalChicken), typeof(EntityAnimalChicken) },
	   { nameof(EntityAnimalBear), typeof(EntityAnimalBear) },
	   { nameof(EntityAnimalZombieBear), typeof(EntityAnimalZombieBear) },
	   { nameof(EntityAnimalWolf), typeof(EntityAnimalWolf) },
	   { nameof(EntityAnimalDireWolf), typeof(EntityAnimalDireWolf) },
	   { nameof(EntityAnimalCoyote), typeof(EntityAnimalCoyote) },
	   { nameof(EntityAnimalMountainLion), typeof(EntityAnimalMountainLion) },
	   { nameof(EntityAnimalBoar), typeof(EntityAnimalBoar) },
	   { nameof(EntityAnimalBossGrace), typeof(EntityAnimalBossGrace) },
	   { nameof(EntityZombieScreamer), typeof(EntityZombieScreamer) },
	   { nameof(EntityZombieSmart), typeof(EntityZombieSmart) },

		// vanilla added to avoid the slow-lookup log spam
	   { nameof(EntitySwarm), typeof(EntitySwarm) },
	   { nameof(EntityTurret), typeof(EntityTurret) },
	   { nameof(EntityAlive), typeof(EntityAlive) },
	   { nameof(EntitySurvivor), typeof(EntitySurvivor) },
	   { nameof(EntityVulture), typeof(EntityVulture) },
	   { nameof(EntityAnimalSnake), typeof(EntityAnimalSnake) }
	};

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EntityFactory.GetEntityType))]
	public static bool Prefix_GetEntityType(string _className, ref Type __result)
	{
		if (EntityTypes.TryGetValue(_className, out __result))
			return false;

		return true;
	}
}
