// Decompiled with JetBrains decompiler
// Type: libWiiSharp.TMD_Content
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

namespace libWiiSharp
{
    public class TMD_Content
    {
        private uint contentId;
        private ushort index;
        private ushort type;
        private ulong size;
        private byte[] hash = new byte[20];

        public uint ContentID
        {
            get => contentId;
            set => contentId = value;
        }

        public ushort Index
        {
            get => index;
            set => index = value;
        }

        public ContentType Type
        {
            get => (ContentType)type;
            set => type = (ushort)value;
        }

        public ulong Size
        {
            get => size;
            set => size = value;
        }

        public byte[] Hash
        {
            get => hash;
            set => hash = value;
        }
    }
}
