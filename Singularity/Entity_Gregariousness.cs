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

namespace Singularity;

public abstract class Gregariousness
{
	public static float assistRadius = 50f;
	public static float maxRadius = 2000f;
	public static float assistCooldownSeconds = 6f;
	public static float alphaCooldownSeconds = 10f;
	public static readonly ConditionalWeakTable<EntityAlive, Data> entities = new();
	public static List<EntityAlive> cachedEntities = new();

	public class Data
	{
		bool doOnce = false;
		public bool IsAlpha { get; set; } = false;
		public bool IsSolitary { get; set; } = false;
		public EntityAlive? MyAlpha { get; set; } = null;
		public float assistCooldown = 0f;
		public float alphaCooldown = 0f;
		public int assistFrameIndex = 0;
		public int alphaFrameIndex = 0;
		public bool IsInAssistCooldown = false;
		public bool IsInAlphaCooldown = false;
		public short slotsLeft = -1;

		public void Init(EntityAlive entity)
		{
			if (doOnce) return;
			doOnce = true;
			float gregariousness = entity.EntityClass.Properties.GetFloat("Singularity_Gregariousness");
			int sLeft = entity.EntityClass.Properties.GetInt("Singularity_GroupSize");
			if (sLeft != 0) slotsLeft = (short)(sLeft - 1);
			IsSolitary = gregariousness == 0f || UnityEngine.Random.value > gregariousness;
		}

		public bool GetInAssistCooldown()
		{
			// all checks in the same frame should return the same value
			if (assistFrameIndex != Time.frameCount)
			{
				assistFrameIndex = Time.frameCount;
				float now = Time.realtimeSinceStartup;
				IsInAssistCooldown = assistCooldown > now;
				if (!IsInAssistCooldown)
					assistCooldown = now + assistCooldownSeconds + UnityEngine.Random.value;
			}
			return IsInAssistCooldown;
		}

		public bool GetInAlphaCooldown()
		{
			if (alphaFrameIndex != Time.frameCount)
			{
				alphaFrameIndex = Time.frameCount;
				float now = Time.realtimeSinceStartup;
				IsInAlphaCooldown = alphaCooldown > now;
				if (!IsInAlphaCooldown)
					alphaCooldown = now + alphaCooldownSeconds + UnityEngine.Random.value;
			}
			return IsInAlphaCooldown;
		}
		public bool SetAlpha(EntityAlive alpha)
		{
			var aData = GetOrCreate(alpha);
			if (aData.slotsLeft != 0)
			{
				--aData.slotsLeft;
				MyAlpha = alpha;
				return true;
			}
			return false;
		}
	}

	public static Data GetOrCreate(EntityAlive entity)
	{
		Data data = entities.GetOrCreateValue(entity);
		data.Init(entity);
		return data;
	}

	public static bool AreThereAlliesAround(EntityAlive entity) => GetMaxAllies(entity);
	public static bool GetNearbyAllies(EntityAlive entity) => GetAlliesInRadius(entity, assistRadius);
	public static bool GetMaxAllies(EntityAlive entity) => GetAlliesInRadius(entity, maxRadius);
	public static bool GetAlliesInRadius(EntityAlive entity, float radius)
	{
		if (cachedEntities.Any()) return true;
		bool found = false;
		var center = entity.position;
		radius = radius * 2f;
		var bb = new Bounds(center, new Vector3(radius, radius, radius));
		cachedEntities = entity.world.GetLivingEntitiesInBounds(entity, bb).Where(ally =>
		{
			if (ally.EntityClass.classname != entity.EntityClass.classname) return false;

			if (ally.aiManager == null) return false;

			Data allyGData = GetOrCreate(ally);
			if (!found) found = !allyGData.IsSolitary;
			return !allyGData.IsSolitary;
		}).ToList();
		return found;
	}
	public static bool TryFindAlpha(EntityAlive entity, out EntityAlive? alpha)
	{
		alpha = null;
		if (!GetMaxAllies(entity)) return false;

		foreach (var e in cachedEntities)
		{
			var eData = GetOrCreate(e);
			if (eData.IsAlpha && eData.slotsLeft != 0)
			{
				alpha = e;
				return true;
			}
		}

		return false;
	}
}
