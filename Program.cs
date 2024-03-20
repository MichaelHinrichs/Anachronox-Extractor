//Written for Anachronox. https://store.steampowered.com/app/242940/
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Anachronox_Extractor
{
    class Program
    {
        static BinaryReader br;
        static void Main(string[] args)
        {
            br = new(File.OpenRead(args[0]));

            if (new string(br.ReadChars(4)) != "ADAT")
                throw new Exception("Not a Anachronox dat file.");

            int fileTable = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();

            br.BaseStream.Position = fileTable;

            List<Subfile> subfiles = new();

            while(br.BaseStream.Position < br.BaseStream.Length)
            {
                subfiles.Add(new());
            }

            foreach (Subfile sub in subfiles)
            {
                Extract(sub, Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]) + "//");
            }
        }

        static void Extract(Subfile sub, string path)
        {
            br.BaseStream.Position = sub.start;
            if (sub.sizeCompressed == 0)
            {
                Directory.CreateDirectory(path + Path.GetDirectoryName(sub.name));
                using FileStream FS = File.Create(path + sub.name);
                BinaryWriter bw = new(FS);
                bw.Write(br.ReadBytes(sub.sizeUncompressed));
                bw.Close();
            }
            else
            {
                br.ReadInt16();
                Directory.CreateDirectory(path + Path.GetDirectoryName(sub.name));
                using var ds = new DeflateStream(new MemoryStream(br.ReadBytes(sub.sizeCompressed - 2)), CompressionMode.Decompress);
                ds.CopyTo(File.Create(path + sub.name));
            }
        }

        class Subfile
        {
            public string name = new string(br.ReadChars(0x80)).TrimEnd((char)0x00);
            public int start = br.ReadInt32();
            public int sizeUncompressed = br.ReadInt32();
            public int sizeCompressed = br.ReadInt32();
            public float unknown = br.ReadSingle();
        }
    }
}
