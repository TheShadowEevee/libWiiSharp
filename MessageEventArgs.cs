// Decompiled with JetBrains decompiler
// Type: libWiiSharp.MessageEventArgs
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;

namespace libWiiSharp
{
    public class MessageEventArgs : EventArgs
    {
        private readonly string message;

        public string Message => message;

        public MessageEventArgs(string message)
        {
            this.message = message;
        }
    }
}
