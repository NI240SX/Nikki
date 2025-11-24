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
using System.Threading.Tasks;

namespace Nikki.Support.Undercover.Framework
{

    public class VinylMetaDataManager : Manager<VinylMetaData>
    {
        public override GameINT GameINT => GameINT.Undercover;

        public override string GameSTR => GameINT.Undercover.ToString();

        public override string Name => "VinylMetaData";

        public override bool AllowsNoSerialization => false;

        public override bool IsReadOnly => false;

        public override Alignment Alignment { get; }

        public override Type CollectionType => typeof(VinylMetaData);

        public VinylMetaDataManager(Datamap db) : base(db)
        {
            this.Extender = 5;
            this.Alignment = Alignment.Default;
        }


        internal override void Assemble(BinaryWriter bw, string mark)
        {
            if (this.Count == 0) return;

            this.SortByKey();

            bw.GeneratePadding(mark, this.Alignment);

            bw.WriteEnum(BinBlockID.VinylMetaData);
            var size = this.Count*8;
            bw.Write(size); //SIZE (this.Count * SkinRegion.BaseClassSize + 8)

            //make SkinRegions array
            foreach (var metadata in this) { 
                bw.Write(metadata.BinKey);
                bw.Write(metadata.UnknownFloat);
            }

        }

        internal override void Disassemble(BinaryReader br, Block block)
        {
            if (Block.IsNullOrEmpty(block)) return;
            if (block.BlockID != BinBlockID.VinylMetaData) return;

            this.Capacity = 0;
            for (int loop = 0; loop < block.Offsets.Count; ++loop)
            {

                br.BaseStream.Position = block.Offsets[loop] + 4;
                var blockSize = br.ReadInt32();
                //var blockStart = br.BaseStream.Position;

                this.Capacity += blockSize/8;

                for (int i = 0; i < blockSize/8; i++) {
                    uint key = br.ReadUInt32();
                    float f = br.ReadSingle();
                    this.Add(new VinylMetaData(key, f, this));
                }

            }
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
            var index = this.IndexOf(cname);

            if (index == -1)
            {

                throw new Exception($"Collection named {cname} does not exist");

            }
            else
            {

                if (serialized) this[index].Serialize(bw);
                else throw new NotSupportedException("Collection supports only serialization and no plain export");

            }
        }

        public override void Import(SerializeType type, BinaryReader br)
        {
            var position = br.BaseStream.Position;
            var header = new SerializationHeader();
            header.Read(br);

            var collection = new VinylMetaData();

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