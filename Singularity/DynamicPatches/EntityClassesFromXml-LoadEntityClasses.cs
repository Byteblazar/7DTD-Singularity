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
using System.Xml.Linq;

namespace Singularity.DynamicPatches;

[HarmonyPatch(typeof(EntityClassesFromXml))]
public abstract partial class EntityClassesFromXml_Patches
{
	public static Dictionary<string, string> EntityAssemblyQualifiedNames = new()
	{
	   { nameof(EntityAnimalSupernatural), typeof(EntityAnimalSupernatural).AssemblyQualifiedName },
	   { nameof(EntityAnimalChicken), typeof(EntityAnimalChicken).AssemblyQualifiedName },
	   { nameof(EntityAnimalBear), typeof(EntityAnimalBear).AssemblyQualifiedName },
	   { nameof(EntityAnimalZombieBear), typeof(EntityAnimalZombieBear).AssemblyQualifiedName },
	   { nameof(EntityAnimalWolf), typeof(EntityAnimalWolf).AssemblyQualifiedName },
	   { nameof(EntityAnimalDireWolf), typeof(EntityAnimalDireWolf).AssemblyQualifiedName },
	   { nameof(EntityAnimalCoyote), typeof(EntityAnimalCoyote).AssemblyQualifiedName },
	   { nameof(EntityAnimalMountainLion), typeof(EntityAnimalMountainLion).AssemblyQualifiedName },
	   { nameof(EntityAnimalBoar), typeof(EntityAnimalBoar).AssemblyQualifiedName },
	   { nameof(EntityAnimalBossGrace), typeof(EntityAnimalBossGrace).AssemblyQualifiedName },
	   { nameof(EntityZombieScreamer), typeof(EntityZombieScreamer).AssemblyQualifiedName },
	   { nameof(EntityZombieSmart), typeof(EntityZombieSmart).AssemblyQualifiedName }
	};

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EntityClassesFromXml.LoadEntityClasses))]
	public static void Prefix_LoadEntityClasses(ref XmlFile _xmlFile)
	{
		if (_xmlFile.GetXpathResults("//property[@name='Class']", out var _matchList))
		{
			foreach (var match in _matchList)
			{
				if (match is XElement xmatch)
				{
					if (xmatch.TryGetAttribute("value", out var typeName))
					{
						if (EntityAssemblyQualifiedNames.TryGetValue(typeName, out string aqn))
						{
							xmatch.SetAttributeValue("value", aqn);
						}
					}
				}
			}
		}
	}
}
