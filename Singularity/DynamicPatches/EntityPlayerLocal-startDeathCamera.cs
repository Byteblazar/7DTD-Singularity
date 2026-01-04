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

public abstract partial class EntityPlayerLocal_Patches
{
	public static bool DeathCam { get; set; }

	public static void Prefix_startDeathCamera(EntityPlayerLocal __instance)
	{
		DeathCam = true;
	}
	public static void Postfix_startDeathCamera(EntityPlayerLocal __instance)
	{
		DeathCam = false;
	}
}
