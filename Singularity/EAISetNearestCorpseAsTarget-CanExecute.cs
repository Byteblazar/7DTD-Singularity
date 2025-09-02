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
	public abstract partial class EAISetNearestCorpseAsTarget_Patches
	{
		public static bool Prefix_CanExecute(EAISetNearestCorpseAsTarget __instance, ref bool __result)
		{
			try
			{
				if (!corpseTypesByTask.TryGetValue(__instance, out var values)
					|| (values.types == null || values.types.Count == 0) && values.cannibalistic)
				{
					return true;
				}

				if (__instance.theEntity.HasInvestigatePosition || __instance.theEntity.IsSleeping || __instance.rndTimeout > 0 && __instance.GetRandom(__instance.rndTimeout) != 0)
				{
					__result = false;
					return false;
				}

				EntityAlive attackTarget = __instance.theEntity.GetAttackTarget();
				if (attackTarget is EntityPlayer && attackTarget.IsAlive() && (double)__instance.RandomFloat < 0.949999988079071)
				{
					__result = false;
					return false;
				}
				__instance.theEntity.world.GetEntitiesAround(__instance.targetFlags, __instance.targetFlags, __instance.theEntity.position, __instance.theEntity.IsSleeper ? 7f : __instance.maxXZDistance, EAISetNearestCorpseAsTarget.entityList);
				EAISetNearestCorpseAsTarget.entityList.Sort((IComparer<Entity>)__instance.sorter);

				bool CandidateAllowed(EntityAlive cand)
				{
					if (cand == null) return false;

					var candidateType = cand.GetType();

					if (!values.cannibalistic && candidateType == __instance.theEntity.GetType()) return false;

					if (values.types != null && values.types.Count > 0)
					{
						foreach (var allowedType in values.types)
						{
							if (allowedType?.IsAssignableFrom(candidateType) == true) return true;
						}
						return false;
					}

					return true;
				}

				EntityAlive? entityAlive = null;
				foreach (Entity e in EAISetNearestCorpseAsTarget.entityList)
				{
					EntityAlive? entity = e as EntityAlive;
					if (entity?.IsDead() == true)
					{
						if (CandidateAllowed(entity))
						{
							entityAlive = entity;
							break;
						}
					}
				}
				EAISetNearestCorpseAsTarget.entityList.Clear();
				__instance.targetEntity = entityAlive;
				__result = __instance.targetEntity != null;
				return false;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return true;
			}
		}
	}

}
