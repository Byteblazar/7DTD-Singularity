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
	public abstract class Explosion_Patches
	{
		public static bool Prefix_AttackEntities(Explosion __instance, int _entityThatCausedExplosion, ItemValue _itemValueExplosionSource)
		{
			if (_itemValueExplosionSource == null
			 || !_itemValueExplosionSource.ItemClass.Properties
					.GetBool("Singularity_IsNonlethalExplosive"))
				return true;

			var buffs = __instance.explosionData.BuffActions;
			if (buffs == null || buffs.Count == 0)
				return false;

			Vector3 centerWorld = __instance.worldPos;
			Vector3 centerLocal = centerWorld - Origin.position;
			float radius = __instance.explosionData.EntityRadius;

			var cols = Physics.OverlapSphere(
				centerLocal,
				radius,
				Physics.AllLayers,
				QueryTriggerInteraction.Collide
			);

			foreach (var col in cols)
			{
				var entity = col.GetComponentInParent<EntityAlive>();
				if (entity == null || entity.IsDead())
					continue;

				Vector3 targetPos = entity.GetPosition();
				Vector3 dir = targetPos - centerWorld;
				float dist = dir.magnitude;
				var ray = new Ray(centerWorld, dir / dist);

				if (Voxel.Raycast(
						__instance.world,
						ray,
						dist,
						65536,   // layerMask for terrain
						66,      // blockMask for solid
						0f       // sphereRadius (vanilla uses 0)
					))
					continue;  // blocked by terrain

				foreach (var buff in buffs)
					entity.Buffs.AddBuff(buff, _entityThatCausedExplosion);
			}

			return false;
		}
	}
}
