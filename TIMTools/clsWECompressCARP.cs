using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace TIMTools
{
    public class clsWECompressCARP
    {
       


        private enum TokenType
        {
            UNASSIGNED = -1,
            RAW = 3,
            SHORT = 2,
            LONG = 0
        }
        private Dictionary<string, object> defaultToken = new Dictionary<string, object>()
    {
        {"data", null},
        {"length", 0},
        {"offset", 0},
        {"type", TokenType.UNASSIGNED}
    };
        private Dictionary<string, int> FindMatch(byte[] buffer_slice, int lowindex, int offsetIndex, int length, int matchMinLength, int matchMaxLength)
        {
            int bestOffset = -1;
            int bestLength = matchMinLength;

            for (int idx = Math.Max(0, lowindex); idx < offsetIndex; idx++)
            {
                int matchLength = 0;
                int matchIdx = 0;

                while (matchIdx < matchMaxLength && matchIdx < length && offsetIndex + matchIdx < length)
                {
                    if (buffer_slice[idx + matchIdx] != buffer_slice[offsetIndex + matchIdx])
                    {
                        break;
                    }

                    matchLength++;
                    matchIdx++;
                }

                if (matchLength >= bestLength)
                {
                    bestOffset = idx;
                    bestLength = matchLength;
                }
            }

            return new Dictionary<string, int>()
        {
            {"offset", bestOffset},
            {"length", bestOffset < 0 ? 0 : bestLength}
        };
        }
        private void WriteCommandBit(Dictionary<string, object> writer, bool flag, List<byte> bytes)
        {

            writer[
                "control"
            ] = Convert.ToInt32(writer["control"]) >> 1;

            if (flag)
            {

                writer[
                    "control"
                ] = Convert.ToInt32(writer["control"]) | 0x80;
            }

          ((List<byte>)writer[
               "buffer"
           ]).AddRange(bytes);

            writer[
                "bits"
            ] = (int)writer[
                "bits"
            ] - 1;
            if ((int)writer[
                   "bits"
               ] != 0)
            {

                return;
            }

            writer[
                "bits"
            ] = 8;
            ((List<byte>)writer[
                 "output"
             ]).Add((byte)(int)writer[
                 "control"
             ]);
            ((List<byte>)writer[
                 "output"
             ]).AddRange((List<byte>)writer[
                 "buffer"
             ]);
            writer[
                "buffer"
            ] = new List<byte>();
            writer[
                "control"
            ] = 0;
        }
        public List<Dictionary<string, object>> GetTokens(List<byte> buffer, byte[] buffer_slice)
        {
            int SIZE = buffer_slice.Length;
            int offset = 0;

            List<Dictionary<string, object>> tokens = new List<Dictionary<string, object>>();
            Dictionary<string, object> token = new Dictionary<string, object>(defaultToken);

            while (offset < SIZE)
            {
                Dictionary<string, int> shortMatch = FindMatch(buffer_slice, offset - 0x10, offset, SIZE, 2, 5);
                Dictionary<string, int> longMatch = FindMatch(buffer_slice, offset - 0x3FF, offset, SIZE, 3, 34);

                bool useShortMatch = shortMatch["offset"] >= 0 && shortMatch["length"] > 0 && shortMatch["length"] >= longMatch["length"];
                bool useLongMatch = longMatch["offset"] >= 0 && longMatch["length"] > 0 && longMatch["length"] > shortMatch["length"];

                if (useShortMatch)
                {
                    if ((TokenType)token[
                         "type"
                     ] != TokenType.UNASSIGNED)
                    {
                        tokens.Add(token);
                    }

                    token = new Dictionary<string,
                        object>(defaultToken);

                    tokens.Add(new Dictionary<string,
                        object>()
                {
                    {"data",
                        null},
                    {"length",
                        shortMatch[
                            "length"
                        ]},
                    {"offset",
                        offset-shortMatch[
                            "offset"
                        ]},
                    {"type",
                        TokenType.SHORT}
                });

                    offset += shortMatch[
                        "length"
                    ];
                }
                else if (useLongMatch)
                {
                    if ((TokenType)token[
                         "type"
                     ] != TokenType.UNASSIGNED)
                    {
                        tokens.Add(token);
                    }

                    token = new Dictionary<string,
                        object>(defaultToken);

                    tokens.Add(new Dictionary<string,
                        object>()
                {
                    {"data",
                        null},
                    {"length",
                        longMatch[
                            "length"
                        ]},
                    {"offset",
                        offset-longMatch[
                            "offset"
                        ]},
                    {"type",
                        TokenType.LONG}
                });

                    offset += longMatch[
                        "length"
                    ];
                }
                else
                {
                    if ((TokenType)token[
                         "type"
                     ] == TokenType.UNASSIGNED)
                    {
                        token[
                            "type"
                        ] = TokenType.RAW;
                        token[
                            "data"
                        ] = new List<byte>();
                    }

                    ((List<byte>)token[
                         "data"
                     ]).Add(buffer_slice[offset]);
                    offset++;

                    // Maximum block length 0x46
                    if (((List<byte>)token[
                         "data"
                     ]).Count == 70)
                    {
                        tokens.Add(token);
                        token = new Dictionary<string,
                            object>(defaultToken);
                    }
                }
            }

            return tokens;
        }
        public List<byte> EncodeTokens(List<Dictionary<string, object>> tokens)
        {

            Dictionary<string,
                object> writer = new Dictionary<string,
                object>()
            {

          {"output",
              new List<byte>()},
          {"control",
              (byte)0},
          {"bits",
              8},
          {"buffer",
              new List<byte>()}
            };

            foreach (Dictionary<string,
                object> token in tokens)
            {

                if ((TokenType)token[
                       "type"
                   ] == TokenType.SHORT)
                {

                    int length = (((int)token[
                        "length"
                    ] - 2) << 4);
                    int distance = (int)token[
                        "offset"
                    ] - 1;
                    WriteCommandBit(writer,
                        true,
                        new List<byte>() { (byte)(0x80 | length | distance) });
                }
                else if ((TokenType)token[
                         "type"
                     ] == TokenType.LONG)
                {

                    int length = (((int)token[
                        "length"
                    ] - 3) << 10);
                    int distance = (int)token[
                        "offset"
                    ];
                    int composite = length | distance;
                    WriteCommandBit(writer,
                        true,
                        new List<byte>() { (byte)(composite>>8),
                      (byte)(composite&0xFF) });
                }
                else if ((TokenType)token[
                         "type"
                     ] == TokenType.RAW)
                {

                    if (((List<byte>)token[
                         "data"
                     ]).Count < 8)
                    {

                        foreach (byte data in (List<byte>)token[
                            "data"
                        ])
                        {

                            WriteCommandBit(writer,
                                false,
                                new List<byte>() { data });
                        }
                    }
                    else
                    {

                        WriteCommandBit(writer,
                            true,
                            new List<byte>() { (byte)(0xB8+((List<byte>)token[
                          "data"
                      ]).Count) }.Concat((List<byte>)token[
                                "data"
                            ]).ToList());
                    }
                }
                else
                {

                    throw new Exception("Tokenizer Error");
                }
            }

            WriteCommandBit(writer,
                true,
                new List<byte>() { 0xFF });

            while ((int)writer[
                   "bits"
               ] != 8)
            {

                WriteCommandBit(writer,
                    true,
                    new List<byte>());
            }

            return (List<byte>)writer[
                       "output"
                   ];
        }
        public void SaveData(List<byte> data, string output)
        {
            byte[] buffer;
            byte[] ajustaBuffer = new byte[1];
            double resto;
            using (FileStream archivo = new FileStream(output,
                FileMode.Create))
            {
                buffer = data.ToArray();
                int largoBuffer = buffer.Length;

                // Seteamos el largo correcto de los BIN
                // Largo + 00
                double largoArchivo = ((double)buffer.Length + 1) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);
                if ((resto == 0) || (resto == 0.25) ||
                    (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 1];


                //Largo + 0000
                largoArchivo = ((double)buffer.Length + 2) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);
                if ((resto == 0) || (resto == 0.25) ||
                    (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 2];

                //Largo + 000000
                largoArchivo = ((double)buffer.Length + 3) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);
                if ((resto == 0) || (resto == 0.25) ||
                    (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 3];

                //Largo + 00000000
                largoArchivo = ((double)buffer.Length + 4) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);
                if ((resto == 0) || (resto == 0.25) ||
                    (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 4];

                int i = 0;
                foreach (byte b in data.ToArray())
                {
                    ajustaBuffer[i++] = b;
                }
                archivo.Write(ajustaBuffer, 0, ajustaBuffer.Length);
            }
        }
        public bool SaveFile(string inputFile, string outputFile)
        {
            bool result = false;

            try
            {
                List<byte> buffer;
                byte[] buffer_slice;
                string filePath = inputFile;

                //byte[] b = null;

                //using (FileStream fs = new FileStream(inputFile,FileMode.Open,FileAccess.Read))
                //{
                //    fs.Position = 64;
                //    long s = fs.Length - 1 - 63 ;
                //    b = new byte[s];
                //    fs.Read(b, 0, b.Length);
                //}
                //buffer.AddRange(b);


                buffer = File.ReadAllBytes(filePath).ToList();
                ushort clut_bytes = ushort.Parse($"{buffer[8]:X2}{buffer[9]:X2}", System.Globalization.NumberStyles.HexNumber);
                byte[] clut_bytes_array = BitConverter.GetBytes(clut_bytes);
                clut_bytes = BitConverter.ToUInt16(clut_bytes_array.Reverse().ToArray(), 0);
                int CLUT_SIZE = clut_bytes + 12;
                buffer_slice = buffer.Skip(8 + CLUT_SIZE).ToArray();
                List<Dictionary<string, Object>> tokens = GetTokens(buffer, buffer_slice);
                List<byte> encoding = EncodeTokens(tokens);
                SaveData(encoding, outputFile);
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception (ex.Message);
            }

            return result;
        }






      




    }
}