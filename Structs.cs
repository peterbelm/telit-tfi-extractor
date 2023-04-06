using System.Text;

namespace TelitTfi {
    public struct Header {
        public uint Magic;
        public uint Version;
        public uint NumElements;

        public static Header FromReader(BinaryReader br) {
            return new Header() {
                Magic = br.ReadUInt32(),
                Version = br.ReadUInt32(),
                NumElements = br.ReadUInt32()
            };
        }

        public bool IsValid() => Magic == 0x12345678 && Version >= 1 && Version <= 2;
    }

    public struct Footer {
        public uint Checksum;
        public uint Magic;

        public static Footer FromReader(BinaryReader br) {
            return new Footer() {
                Checksum = br.ReadUInt32(),
                Magic = br.ReadUInt32()
            };
        }

        public bool IsValid() => Magic == 0xEDCBA987; // -0x12345678
    }

    public class Element {
        public uint Tag { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }

        public Element() {
            Group = "";
            Name = "";
        }
    }

    public class ElementV1 : Element {
        public static Element FromReader(BinaryReader br) {
            return new Element() {
                Tag = br.ReadUInt32(),
                Offset = br.ReadUInt32(),
                Length = br.ReadUInt32(),
                Group = Encoding.ASCII.GetString(br.ReadBytes(16).TakeWhile((b) => b != 0x00).ToArray()),
                Name = Encoding.ASCII.GetString(br.ReadBytes(128).TakeWhile((b) => b != 0x00).ToArray()),
            };
        }
    }

    public class ElementV2 : Element {
        public static Element FromReader(BinaryReader br) {
            return new Element() {
                Tag = br.ReadUInt32(),
                Offset = br.ReadUInt32(),
                Length = br.ReadUInt32(),
                Group = Encoding.ASCII.GetString(br.ReadBytes(16).TakeWhile((b) => b != 0x00).ToArray()),
                Name = Encoding.ASCII.GetString(br.ReadBytes(1184).TakeWhile((b) => b != 0x00).ToArray()),
            };
        }
    }
}