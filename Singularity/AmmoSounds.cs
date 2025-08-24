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

namespace Singularity
{
	public class NetPackageItemActionSound : NetPackage
	{
		public int entityId;
		public byte slotIdx;
		public byte actionIdx;
		public string? soundStart;
		public string? soundLoop;

		public NetPackageItemActionSound Setup(int _entityId, int _slotIdx, int _actionIdx, string soundStart, string soundLoop)
		{
			this.entityId = _entityId;
			this.slotIdx = (byte)_slotIdx;
			this.actionIdx = (byte)_actionIdx;
			this.soundStart = soundStart;
			this.soundLoop = soundLoop;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			this.entityId = _reader.ReadInt32();
			this.slotIdx = _reader.ReadByte();
			this.actionIdx = _reader.ReadByte();
			this.soundStart = _reader.ReadString();
			this.soundLoop = _reader.ReadString();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(entityId);
			_writer.Write(slotIdx);
			_writer.Write(actionIdx);
			_writer.Write(soundStart);
			_writer.Write(soundLoop);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (Utils.IsHost) SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(this, _allButAttachedToEntityId: entityId);
			if (!GameManager.IsDedicatedServer)
			{
				EntityAlive? entity = (EntityAlive?)_world?.GetEntity(entityId);
				if (entity == null) return;

				var actionData = entity.inventory.GetItemActionDataInSlot(slotIdx, actionIdx) as ItemActionRanged.ItemActionDataRanged;
				if (actionData == null) return;

				if (!string.IsNullOrWhiteSpace(soundStart)) actionData.SoundStart = soundStart;
				if (!string.IsNullOrWhiteSpace(soundLoop)) actionData.SoundLoop = soundLoop;
			}
		}

		public override int GetLength() => 100;
	}
}
