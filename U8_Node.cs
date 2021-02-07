// Decompiled with JetBrains decompiler
// Type: libWiiSharp.U8_Node
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.IO;

namespace libWiiSharp
{
    public class U8_Node
    {
        private ushort type;
        private ushort offsetToName;
        private uint offsetToData;
        private uint sizeOfData;

        public U8_NodeType Type
        {
            get => (U8_NodeType)type;
            set => type = (ushort)value;
        }

        public ushort OffsetToName
        {
            get => offsetToName;
            set => offsetToName = value;
        }

        public uint OffsetToData
        {
            get => offsetToData;
            set => offsetToData = value;
        }

        public uint SizeOfData
        {
            get => sizeOfData;
            set => sizeOfData = value;
        }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(type)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToName)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToData)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(sizeOfData)), 0, 4);
        }
    }
}
