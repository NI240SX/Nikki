using System;
using System.IO;
using System.ComponentModel;
using Nikki.Core;
using Nikki.Utils;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Attributes;
using Nikki.Support.Undercover.Framework;
using CoreExtensions.IO;
using CoreExtensions.Conversions;
using Nikki.Reflection.Interface;



namespace Nikki.Support.Undercover.Class
{
    public class EventSequence : Collectable, IAssembly
    {

        #region Properties

        [Browsable(false)]
        public override GameINT GameINT => GameINT.Undercover;

        [Browsable(false)]
        public override string GameSTR => GameINT.Undercover.ToString();

        [Browsable(false)]
        public EventSequenceManager Manager { get; set; }

        private string _collection_name;
        [AccessModifiable()]
        [Category("Main")]
        public override string CollectionName {
            get => this._collection_name;
            set {
                this.Manager?.CreationCheck(value);
                this._collection_name = value;
            }
        }

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint BinKey => this._collection_name.BinHash();

        [Category("Main")]
        [TypeConverter(typeof(HexConverter))]
        public uint VltKey => this._collection_name.VltHash();

        public uint Unknown1;
        [Category("Primary")]
        public uint ElementsCount { get => _elementsCount; }
        public uint _elementsCount;
        public uint Unknown3;

        [Category("Primary")]
        public string Status { get => "EventSequence data not supported."; }

        #endregion

        #region Main

        public EventSequence() { }

        public EventSequence(string CName, EventSequenceManager manager) {
            this.Manager = manager;
            this.CollectionName = CName;
            CName.VltHash();
        }

        public EventSequence(BinaryReader br, EventSequenceManager manager) {
            this.Manager = manager;
            this.Disassemble(br);
            this.CollectionName.BinHash();
        }

        #endregion

        #region Methods

        public void Assemble(BinaryWriter bw) {
            //throw new NotImplementedException();
        }

        public void Disassemble(BinaryReader br) {

            while (br.ReadUInt32() == 0x11111111) { }
            br.BaseStream.Position -= 4; //skip alignment

            // inner data length (block length - 24)=544
            var innerDataLength = br.ReadUInt32();
            br.ReadUInt32(); //0
            br.ReadUInt32(); //0
            br.ReadUInt32(); //0

            if (br.ReadUInt32() != 0x43415250) Console.WriteLine("EventSequence Warning Not a PRAC block");
            Unknown1 = br.ReadUInt32();
            _elementsCount = br.ReadUInt32();
            Unknown3 = br.ReadUInt32();

            //TODO
            br.BaseStream.Position += (_elementsCount-1) * 16 + 12;
            var distance = br.ReadUInt32();
            br.BaseStream.Position += distance - 16;
            _collection_name = br.ReadNullTermUTF8();



            /*
            alright
            block 11B80300 is an EventSequence entry (299 records)
            start
            0x11111111 11111111


            50524143 PRAC 0x43415250
            int=26
            int=7 number of entries in the table +1
            int=1

            73727453 02220000 04000000 F8000000
            srtS     8706     4        248
            8706 = (innerDataLength+0.125)*16 but doesnt always work


            table
                              vlt hash
            00004165 03180000 ACCC8271 18010000 
                     6147     Frag_Hid 280
            00004565 03140000 B9211C2E 28010000 
                     5123     [name]   296
            00005365 03100000 B189D2E9 38010000 
                     4099     System   312
            00005465 030C0000 6B129629 38010000 
                     3075     State    312
            00006C65 03600000 768FB836 38010000 
                     24579    Hide_Amb 312
            00004E69 03160000 B9211C2E 88010000
                     5635     [name]   392      distance from table entry to name string

            00000000 00000000 00000000 00000000 
            00000000 00000000 00000000 00000000 
            00000000 00000000 00000000 00000000 
            00000000

                     01010000 08010B00 C3000802 
                     257
            A0176600 D0676900 C8807C00 
                                       00000000 
            00000000 00000000 00000000 00000000 
            00000000 00000000 00000000 00000000 
            00000000 00000000 00000000 00000000 
            00000000 00000000
            
            State�Hide_Amber�Frag_Hide�System�
            0000 00000000 00000000 00000000

            ACCC8271 00000000 01000000 0000803F 
            [name]            1        1.0
            4B7E313F 00006C65 00000000 00000000 
            0.693    
            00004E69 01000000 C12B0000 B189D2E9 
                     1        11201    System
            00005365 F5000802 08607700 D8486000 
            
            6B129629 01000000 6B129629 00005465 
            State    1        State
            00000000 00004165 01000000 00000000 
                              1
            02000000 768FB836 00000000 00000000 
            2        Hide_Amb
            FAD21307 10000000 20000000 00000000 
                     16       32
            DD4FAFC1 20000000 20000000 00000000 
                     32       32
            FAD21307 00000000 00000000 00000000 

            DD4FAFC1 102185A2 00000000 0000803F 
            -21.9                      1.0
            00000000 00000100 00000000 00000000
                     65536

            AmberTraffic_Frag_seq�   [name]

            0000 00000000 
            01010000 DC000B00 9F000802
            257               



            
            name at 536/528/504

             */
        }

        /// <summary>
        /// Casts all attributes from this object to another one.
        /// </summary>
        /// <param name="CName">CollectionName of the new created object.</param>
        /// <returns>Memory casted copy of the object.</returns>
        public override Collectable MemoryCast(string CName) {
            var result = new EventSequence(CName, this.Manager);
            base.MemoryCast(this, result);
            return result;
        }

        public override string ToString()
        {
            return $"Collection Name: {this.CollectionName} | " +
                   $"BinKey: {this.BinKey:X8} | Game: {this.GameSTR}";
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes instance into a byte array and stores it in the file provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public void Serialize(BinaryWriter bw)
        {
            return;
            //throw new NotImplementedException();
            byte[] array;
            using (var ms = new MemoryStream(0xB0))
            using (var writer = new BinaryWriter(ms))
            {

                writer.WriteNullTermUTF8(this._collection_name);

                array = ms.ToArray();

            }

            array = Interop.Compress(array, LZCompressionType.RAWW);

            var header = new SerializationHeader(array.Length, this.GameINT, this.Manager.Name);
            header.Write(bw);
            bw.Write(array.Length);
            bw.Write(array);
        }

        /// <summary>
        /// Deserializes byte array into an instance by loading data from the file provided.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        public void Deserialize(BinaryReader br)
        {
            return;
            //throw new NotImplementedException();
            var size = br.ReadInt32();
            var array = br.ReadBytes(size);

            array = Interop.Decompress(array);

            using var ms = new MemoryStream(array);
            using var reader = new BinaryReader(ms);

            this._collection_name = reader.ReadNullTermUTF8();
        }

        #endregion
    }
}