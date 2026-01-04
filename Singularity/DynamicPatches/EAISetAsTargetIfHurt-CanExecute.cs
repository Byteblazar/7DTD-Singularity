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
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Singularity.DynamicPatches;

public abstract partial class EAISetAsTargetIfHurt_Patches
{
	// This transpiler removes the requirement for entities to be of different entityType (enum)
	public static IEnumerable<CodeInstruction> Transpiler_CanExecute(IEnumerable<CodeInstruction> instructions)
	{
		var codes = instructions.ToList();

		for (int i = 0; i < codes.Count - 5; i++)
		{
			var c0 = codes[i];
			bool isLdloc =
				c0.opcode == OpCodes.Ldloc_0 || c0.opcode == OpCodes.Ldloc_1 || c0.opcode == OpCodes.Ldloc_2 ||
				c0.opcode == OpCodes.Ldloc_3 || c0.opcode == OpCodes.Ldloc_S || c0.opcode == OpCodes.Ldloc;

			if (!isLdloc) continue;

			var c1 = codes[i + 1];
			if (c1.opcode != OpCodes.Ldfld || c1.operand is not FieldInfo f1 || !string.Equals(f1.Name, "entityType", StringComparison.Ordinal))
				continue;

			var c2 = codes[i + 2];
			if (c2.opcode != OpCodes.Ldarg_0) continue;

			var c3 = codes[i + 3];
			if (c3.opcode != OpCodes.Ldfld || c3.operand is not FieldInfo f3 || !string.Equals(f3.Name, "theEntity", StringComparison.Ordinal))
				continue;

			var c4 = codes[i + 4];
			if (c4.opcode != OpCodes.Ldfld || c4.operand is not FieldInfo f4 || !string.Equals(f4.Name, "entityType", StringComparison.Ordinal))
				continue;

			var c5 = codes[i + 5];
			if (!(c5.opcode == OpCodes.Beq || c5.opcode == OpCodes.Beq_S)) continue;

			var collected = new List<Label>();
			for (int j = i; j <= i + 4; j++)
			{
				if (codes[j].labels != null && codes[j].labels.Count > 0)
					collected.AddRange(codes[j].labels);
				codes[j].labels = new List<Label>();
			}

			if (codes[i + 5].labels == null) codes[i + 5].labels = new List<Label>();
			codes[i + 5].labels.AddRange(collected);

			codes.RemoveRange(i, 5);

			var old = codes[i];
			var replacement = new CodeInstruction(OpCodes.Nop)
			{
				labels = old.labels != null ? new List<Label>(old.labels) : new List<Label>(),
				blocks = old.blocks != null ? new List<ExceptionBlock>(old.blocks) : null
			};
			codes[i] = replacement;

			Debug.Log("[Transpiler] Removed entityType equality check in EAISetAsTargetIfHurt.CanExecute()");
			break;
		}

		return codes.AsEnumerable();
	}
}
