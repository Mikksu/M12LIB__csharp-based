using System.Collections.Generic;
using System.IO;

namespace M12.Base
{
    public class MemoryBlock
    {
        public MemoryBlock(byte[] Data)
        {
            using (MemoryStream stream = new MemoryStream(Data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    Sequence = reader.ReadUInt16();
                    Length = (ushort)(reader.ReadUInt16() / 2);

                    if (Length > 0)
                    {
                        Values = new List<short>();

                        for (int i = 0; i < Length; i++)
                        {
                            Values.Add(reader.ReadInt16());
                        }
                    }
                }
            }
        }

        public ushort Sequence { get; set; }

        public ushort Length { get; set; }

        public List<short> Values { get; set; }


    }
}
