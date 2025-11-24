using CoreExtensions.IO;
using Nikki.Core;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Exception;
using Nikki.Reflection.Interface;
using Nikki.Support.Undercover.Class;
using Nikki.Support.Undercover.Parts.SkinParts;
using Nikki.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Nikki.Support.Undercover.Framework
{

    public class EventSequenceManager : Manager<EventSequence>
    {
        public override GameINT GameINT => GameINT.Undercover;

        public override string GameSTR => GameINT.Undercover.ToString();

        public override string Name => "EventSequence";

        public override bool AllowsNoSerialization => true;

        internal bool _readonly = true;
        public override bool IsReadOnly => _readonly;

        public override Alignment Alignment { get; }

        public override Type CollectionType => typeof(EventSequence);

        public EventSequenceManager(Datamap db) : base(db)
        {
            this.Extender = 50;
            this.Alignment = Alignment.Default; //could be 0x10 or 0x20, to check
        }


        internal override void Assemble(BinaryWriter bw, string mark)
        {
            if (this.Count == 0) return;

            throw new NotImplementedException();

            this.SortByKey();

        }

        internal override void Disassemble(BinaryReader br, Block block)
        {
            _readonly = false;
            this.Capacity = 0;
            if (Block.IsNullOrEmpty(block)) return;
            if (block.BlockID != BinBlockID.EventSequence) return;

            for (int loop = 0; loop < block.Offsets.Count; ++loop) {

                br.BaseStream.Position = block.Offsets[loop] + 4;
                var blockSize = br.ReadInt32();
                var blockStart = br.BaseStream.Position;

                while (br.BaseStream.Position < blockStart + blockSize)
                {
                    uint innerBlockType = br.ReadUInt32();
                    var innerBlockLength = br.ReadInt32();
                    var innerBlockStart = br.BaseStream.Position;

                    if (innerBlockType != (uint)BinBlockID.NISScript) {
                        Console.WriteLine($"EventSequence : Not a NIS script ! Skipping block type {(BinBlockID)innerBlockType}");
                        br.BaseStream.Position = innerBlockStart + innerBlockLength;
                        continue;
                    }

                    //try { 
                        this.Add(new EventSequence(br, this)); 
                    //} catch { }

                    br.BaseStream.Position = innerBlockStart + innerBlockLength;
                }

            }
            this.Capacity = this.Count;
            _readonly = true;
        }

        internal override void CreationCheck(string cname)
        {
            if (String.IsNullOrWhiteSpace(cname))
            {

                throw new ArgumentNullException("CollectionName cannot be null, empty or whitespace");

            }

            if (cname.Contains(" "))
            {

                throw new ArgumentException("CollectionName cannot contain whitespace");

            }

            if (this.Find(cname) != null)
            {

                throw new CollectionExistenceException(cname);

            }
        }

        public override void Export(string cname, BinaryWriter bw, bool serialized = true)
        {
            return;
            var index = this.IndexOf(cname);

            if (index == -1)
            {

                throw new Exception($"Collection named {cname} does not exist");

            }
            else
            {

                if (serialized) this[index].Serialize(bw);
                else this[index].Assemble(bw); 
                //throw new NotSupportedException("Collection supports only serialization and no plain export");

            }
        }

        public override void Import(SerializeType type, BinaryReader br)
        {
            return;
            var position = br.BaseStream.Position;
            var header = new SerializationHeader();
            header.Read(br);

            var collection = new EventSequence();

            if (header.ID != BinBlockID.Nikki)
            {

                throw new Exception($"Missing serialized header in the imported collection");
                //br.BaseStream.Position = position;
                //collection.Disassemble(br);

            }
            else
            {

                if (header.Game != this.GameINT)
                {

                    throw new Exception($"Stated game inside collection is {header.Game}, while should be {this.GameINT}");

                }

                if (header.Name != this.Name)
                {

                    throw new Exception($"Imported collection is not a collection of type {this.Name}");

                }

                collection.Deserialize(br);

            }

            var index = this.IndexOf(collection);

            if (index == -1)
            {

                collection.Manager = this;
                this.Add(collection);

            }
            else
            {

                switch (type)
                {
                    case SerializeType.Negate:
                        break;

                    case SerializeType.Synchronize:
                    case SerializeType.Override:
                        collection.Manager = this;
                        this.Replace(collection, index);
                        break;

                    //case SerializeType.Override:
                    //    collection.Manager = this;
                    //    this.Replace(collection, index);
                    //    break;

                    //case SerializeType.Synchronize:
                    //    this[index].Synchronize(collection);
                    //    break;

                    default:
                        break;
                }

            }
        }





    }
}