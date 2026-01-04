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

namespace Singularity.DynamicPatches;

public abstract partial class EAISetNearestCorpseAsTarget_Patches
{
	public class EntityValues
	{
		public HashSet<Type> types = new();
		public bool cannibalistic = false;  //if not cannibal, will never eat corpses of the same type as self
	}

	public static readonly ConditionalWeakTable<EAISetNearestCorpseAsTarget, EntityValues> corpseTypesByTask = new();
	public static void Postfix_SetData(EAISetNearestCorpseAsTarget __instance, DictionarySave<string, string> data)
	{
		try
		{
			if (data == null) return;
			var values = corpseTypesByTask.GetOrCreateValue(__instance);

			if (data.TryGetValue("class", out var classes) && !string.IsNullOrWhiteSpace(classes))
			{
				var parts = classes.Split(',', StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					values.types.Add(EntityFactory.GetEntityType(part));
				}
			}

			if (data.TryGetValue("cannibalismChance", out var chance) && !string.IsNullOrWhiteSpace(chance))
			{
				var cannibalismChance = StringParsers.ParseFloat(chance);
				values.cannibalistic = cannibalismChance >= UnityEngine.Random.value;
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}
}
