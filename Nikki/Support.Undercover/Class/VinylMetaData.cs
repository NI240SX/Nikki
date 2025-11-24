using CoreExtensions.Conversions;
using CoreExtensions.IO;
using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Attributes;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Interface;
using Nikki.Support.Undercover.Framework;
using Nikki.Support.Undercover.Parts.SkinParts;
using Nikki.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Nikki.Support.Undercover.Class
{
    public class VinylMetaData : Collectable, IAssembly
    {
        #region Fields
        private string _collection_name;
        [AccessModifiable()]
        [Category("Main")]
        public override string CollectionName
        {
            get => this._collection_name;
            set
            {
                this.Manager?.CreationCheck(value);
                this._collection_name = value;
            }
        }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint BinKey => this._collection_name.BinHash();

        [Browsable(false)]
        public override GameINT GameINT => GameINT.Undercover;

        [Browsable(false)]
        public override string GameSTR => GameINT.Undercover.ToString();

        [Browsable(false)]
        public VinylMetaDataManager Manager { get; set; }

        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Primary")]
        public float UnknownFloat { get; set; }

        #endregion

        #region Main
        public VinylMetaData()
        {
        }
        public VinylMetaData(string Name, VinylMetaDataManager mgr)
        {
            this.Manager = mgr;
            this.CollectionName = Name;
        }

        public VinylMetaData(uint key, float f, VinylMetaDataManager mgr)
        {
            this.Manager = mgr;
            this.CollectionName = key.BinString(LookupReturn.EMPTY);
            this.UnknownFloat = f;
        }
        #endregion

        #region Methods
        public override Collectable MemoryCast(string CName)
        {
            var result = new VinylMetaData(CName, this.Manager)
            {
                UnknownFloat = this.UnknownFloat
            };

            return result;
        }

        public void Assemble(BinaryWriter bw) =>throw new NotImplementedException();
        public void Disassemble(BinaryReader br) =>throw new NotImplementedException();

        public void Serialize(BinaryWriter bw)
        {
            byte[] array;
            using (var ms = new MemoryStream(0x20))
            using (var writer = new BinaryWriter(ms))
            {

                writer.WriteNullTermUTF8(this._collection_name);
                writer.Write(this.UnknownFloat);
                array = ms.ToArray();

            }

            array = Interop.Compress(array, LZCompressionType.RAWW);

            var header = new SerializationHeader(array.Length, this.GameINT, this.Manager.Name);
            header.Write(bw);
            bw.Write(array.Length);
            bw.Write(array);
        }

        public void Deserialize(BinaryReader br)
        {
            int size = br.ReadInt32();
            var array = br.ReadBytes(size);

            array = Interop.Decompress(array);

            using var ms = new MemoryStream(array);
            using var reader = new BinaryReader(ms);

            this._collection_name = reader.ReadNullTermUTF8();
            this.UnknownFloat = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"Collection Name: {this.CollectionName} | " +
                   $"BinKey: {this.BinKey:X8} | Game: {this.GameSTR}";
        }
        #endregion

    }
}