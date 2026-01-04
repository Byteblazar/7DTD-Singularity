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

namespace Singularity.DynamicPatches;

public abstract partial class EntityAlive_Patches
{
	public static void Postfix_SetAttackTarget(EntityAlive __instance, EntityAlive _attackTarget, int _attackTargetTime)
	{
		try
		{
			if (_attackTarget?.IsAlive() != true
				|| __instance?.IsAlive() != true
				|| __instance == _attackTarget) return;

			Gregariousness.Data attackerGData = Gregariousness.GetOrCreate(__instance);
			if (attackerGData.GetInAssistCooldown()) return;

			Gregariousness.Data targetGData = Gregariousness.GetOrCreate(_attackTarget);
			if (targetGData.IsSolitary || targetGData.GetInAssistCooldown()) return;

			Gregariousness.cachedEntities.Clear();
			if (!Gregariousness.GetNearbyAllies(_attackTarget)) return;

			var attackerType = __instance.GetType();

			foreach (var ally in Gregariousness.cachedEntities)
			{
				var targetTasks = ally.aiManager.GetTargetTasks<EAISetNearestEntityAsTarget>();
				if (targetTasks == null) continue;

				foreach (var ttask in targetTasks)
				{
					if (ttask == null) continue;

					ttask.targetClasses ??= new List<EAISetNearestEntityAsTarget.TargetClass>();

					bool exists = false;
					foreach (var tc in ttask.targetClasses)
					{
						if (tc.type == attackerType)
						{
							exists = true;
							break;
						}
					}

					if (!exists)
					{
						var newTc = new EAISetNearestEntityAsTarget.TargetClass
						{
							type = attackerType,
							hearDistMax = Gregariousness.assistRadius,
							seeDistMax = Gregariousness.assistRadius
						};
						ttask.targetClasses.Add(newTc);
					}
				}
				if (ally.GetRevengeTarget() == null && !IsEntityBusy(ally)) { ally.SetRevengeTarget(__instance); }
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	public static bool IsEntityBusy(EntityAlive _entity)
	{
		if (_entity.IsEating) return true;
		var mgr = _entity.aiManager;
		var execList = mgr?.tasks?.executingTasks;
		if (execList != null && execList.Count > 0)
		{
			for (int ei = 0; ei < execList.Count; ei++)
			{
				var entry = execList[ei];
				if (entry == null || entry.action == null) continue;
				var a = entry.action;

				if (a is EAIApproachAndAttackTarget
						|| a is EAIRangedAttackTarget
						|| a is EAIDestroyArea
						|| a is EAIBreakBlock
						|| a is EAIApproachDistraction
						|| a is EAIRunAway
				) return true;
			}
		}
		return false;
	}
}
