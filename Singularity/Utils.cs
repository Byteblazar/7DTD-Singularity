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
	public static class Utils
	{
		public static bool IsHost => GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		public static bool IsMultiplayerHost => (!GameManager.IsDedicatedServer) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		public static Color ParseAnyColor(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return Color.clear;

			input = input.Trim();

			try
			{
				return StringParsers.ParseHexColor(input);
			}
			catch
			{
				return StringParsers.ParseColor(input);
			}
		}
	}
}
