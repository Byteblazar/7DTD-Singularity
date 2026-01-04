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
public abstract partial class XUiC_OptionsVideo_Patches
{
	public static float Target { get; set; } = 0f;
	public static void Finalizer_Init(ref XUiC_OptionsVideo __instance)
	{
		__instance.comboBrightness.ViewComponent.IsVisible = false;
		__instance.comboBrightness.IsDormant = true;
		__instance.btnDefaultBrightness.ViewComponent.IsVisible = false;
		__instance.btnDefaultBrightness.IsDormant = true;
	}
}
public abstract partial class XUiC_OptionsVideoSimplified_Patches
{
	public static void Finalizer_Init(ref XUiC_OptionsVideoSimplified __instance)
	{
		__instance.comboBrightness.ViewComponent.IsVisible = false;
		__instance.comboBrightness.IsDormant = true;
		__instance.btnDefaultBrightness.ViewComponent.IsVisible = false;
		__instance.btnDefaultBrightness.IsDormant = true;
	}
}
