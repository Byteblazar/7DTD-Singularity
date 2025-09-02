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

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Singularity
{
	public abstract partial class EntityAlive_Patches
	{
		public static float assistRadius = 100f;
		public static float assistCooldownSeconds = 6f;

		public static readonly List<Entity> entitiesFound = new();
		public static readonly ConditionalWeakTable<EntityAlive, FloatBox> cooldownsByAttacker = new();
		public static readonly ConditionalWeakTable<EntityAlive, FloatBox> cooldownsByTarget = new();
		public static readonly ConditionalWeakTable<EntityAlive, FloatBox> rollsByAlly = new();
		public class FloatBox { public float value = 0f; }

		public static void Postfix_SetAttackTarget(EntityAlive __instance, EntityAlive _attackTarget, int _attackTargetTime)
		{
			try
			{
				if (_attackTarget?.IsAlive() != true
					|| __instance?.IsAlive() != true
					|| __instance == _attackTarget) return;

				float now = Time.realtimeSinceStartup;
				var cooldown = cooldownsByAttacker.GetOrCreateValue(__instance);
				if (cooldown.value > now) return;
				cooldown.value = now + assistCooldownSeconds + UnityEngine.Random.value;

				cooldown = cooldownsByTarget.GetOrCreateValue(_attackTarget);
				if (cooldown.value > now) return;
				cooldown.value = now + assistCooldownSeconds + UnityEngine.Random.value;

				float gregariousness = _attackTarget.EntityClass.Properties.GetFloat("Singularity_Gregariousness");

				if (gregariousness <= 0) return;

				var center = _attackTarget.position;
				var bb = new Bounds(center, new Vector3(assistRadius * 2f, 6f, assistRadius * 2f));

				_attackTarget.world.GetEntitiesInBounds(typeof(EntityAlive), bb, entitiesFound);

				var attackerType = __instance.GetType();

				foreach (var entity in entitiesFound)
				{
					var ally = entity as EntityAlive;
					if (ally == null || ally == _attackTarget || !ally.IsAlive()) continue;

					if (ally.entityClass != _attackTarget.entityClass) continue;

					var mgr = ally.aiManager;
					if (mgr == null) continue;

					var roll = rollsByAlly.GetOrCreateValue(ally);
					if (roll.value == 0f) roll.value = UnityEngine.Random.value;
					if (roll.value > gregariousness) continue;

					var targetTasks = mgr.GetTargetTasks<EAISetNearestEntityAsTarget>();
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
								hearDistMax = assistRadius,
								seeDistMax = assistRadius
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
			finally
			{
				entitiesFound.Clear();
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
}
