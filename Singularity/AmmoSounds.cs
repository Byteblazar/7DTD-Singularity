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

using System.Collections.Concurrent;

namespace Singularity
{
    public class NetPackageItemActionSound : NetPackage
    {
        public int entityId;
        public byte slotIdx;
        public byte actionIdx;
        public int ammoItemId;

        public NetPackageItemActionSound Setup(int _entityId, int _slotIdx, int _actionIdx, int _ammoItemId)
        {
            this.entityId = _entityId;
            this.slotIdx = (byte)_slotIdx;
            this.actionIdx = (byte)_actionIdx;
            this.ammoItemId = _ammoItemId;
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            this.entityId = _reader.ReadInt32();
            this.slotIdx = _reader.ReadByte();
            this.actionIdx = _reader.ReadByte();
            this.ammoItemId = _reader.ReadInt32();
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            _writer.Write(this.entityId);
            _writer.Write(this.slotIdx);
            _writer.Write(this.actionIdx);
            _writer.Write(this.ammoItemId);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (Utils.IsHost)
                return;

            EntityAlive? entity = (EntityAlive?)_world?.GetEntity(this.entityId);
            if (entity == null) return;

            var actionData = entity.inventory.GetItemActionDataInSlot(slotIdx, actionIdx) as ItemActionRanged.ItemActionDataRanged;
            if (actionData == null) return;

            var sInfo = SingularitySoundLookup.GetSoundInfo(this.ammoItemId);

            string? ammoSoundStart = sInfo?.Start;
            string? ammoSoundLoop = sInfo?.Loop;

            if (string.IsNullOrEmpty(ammoSoundStart))
            {
                var props = actionData.invData.itemValue.ItemClass.Actions[actionIdx].Properties;
                props.Values.TryGetValue("Sound_start", out ammoSoundStart);
            }
            if (string.IsNullOrEmpty(ammoSoundLoop))
            {
                var props = actionData.invData.itemValue.ItemClass.Actions[actionIdx].Properties;
                props.Values.TryGetValue("Sound_loop", out ammoSoundLoop);
            }

            actionData.SoundStart = ammoSoundStart;
            actionData.SoundLoop = ammoSoundLoop;
        }

        public override int GetLength() => 20;
    }

    public sealed class SoundInfo
    {
        public readonly string Start;
        public readonly string Loop;
        //public readonly string End;
        //public readonly string Empty;

        public SoundInfo(string? start = null, string? loop = null/*, string end = null, string empty = null*/)
        {
            Start = start;
            Loop = loop;
            //End = end;
            //Empty = empty;
        }
    }

    public static class SingularitySoundLookup
    {
        static readonly ConcurrentDictionary<int, SoundInfo> s_cache = new(concurrencyLevel: 2, capacity: 128);

        public static SoundInfo? GetSoundInfo(int ammoItemId)
        {
            if (ammoItemId <= 0) return default;

            return s_cache.GetOrAdd(ammoItemId, id =>
            {
                ItemClass ammoClass = ItemClass.GetForId(id);
                if (ammoClass?.Properties?.Values == null) return new SoundInfo();

                ammoClass.Properties.Values.TryGetValue("Singularity_Sound_start", out var start);
                ammoClass.Properties.Values.TryGetValue("Singularity_Sound_loop", out var loop);
                //ammoClass.Properties.Values.TryGetValue("Singularity_Sound_end", out var end);
                //ammoClass.Properties.Values.TryGetValue("Singularity_Sound_empty", out var empty);

                return new SoundInfo(start, loop/*, end, empty*/);
            });
        }
    }
}
