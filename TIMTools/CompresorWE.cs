using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TIMTools
{
   
    public class CompresorWE
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

        /// <summary>
        /// Encuentra la mejor coincidencia entre dos bloques de datos en un buffer.
        /// </summary>
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

        /// <summary>
        /// Escribe un bit de comando en el flujo de salida.
        /// </summary>
        private void WriteCommandBit(Dictionary<string, object> writer, bool flag, List<byte> bytes)
        {
            writer["control"] = Convert.ToInt32(writer["control"]) >> 1;

            if (flag)
            {
                writer["control"] = Convert.ToInt32(writer["control"]) | 0x80;
            }

            ((List<byte>)writer["buffer"]).AddRange(bytes);

            writer["bits"] = (int)writer["bits"] - 1;
            if ((int)writer["bits"] != 0)
            {
                return;
            }

            writer["bits"] = 8;
            ((List<byte>)writer["output"]).Add((byte)(int)writer["control"]);
            ((List<byte>)writer["output"]).AddRange((List<byte>)writer["buffer"]);
            writer["buffer"] = new List<byte>();
            writer["control"] = 0;
        }

        /// <summary>
        /// Obtiene los tokens (RAW, SHORT, LONG) de los datos de entrada.
        /// </summary>
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
                    if ((TokenType)token["type"] != TokenType.UNASSIGNED)
                    {
                        tokens.Add(token);
                    }

                    token = new Dictionary<string, object>(defaultToken);

                    tokens.Add(new Dictionary<string, object>()
                    {
                        {"data", null},
                        {"length", shortMatch["length"]},
                        {"offset", offset - shortMatch["offset"]},
                        {"type", TokenType.SHORT}
                    });

                    offset += shortMatch["length"];
                }
                else if (useLongMatch)
                {
                    if ((TokenType)token["type"] != TokenType.UNASSIGNED)
                    {
                        tokens.Add(token);
                    }

                    token = new Dictionary<string, object>(defaultToken);

                    tokens.Add(new Dictionary<string, object>()
                    {
                        {"data", null},
                        {"length", longMatch["length"]},
                        {"offset", offset - longMatch["offset"]},
                        {"type", TokenType.LONG}
                    });

                    offset += longMatch["length"];
                }
                else
                {
                    if ((TokenType)token["type"] == TokenType.UNASSIGNED)
                    {
                        token["type"] = TokenType.RAW;
                        token["data"] = new List<byte>();
                    }

                    ((List<byte>)token["data"]).Add(buffer_slice[offset]);
                    offset++;

                    if (((List<byte>)token["data"]).Count == 70)
                    {
                        tokens.Add(token);
                        token = new Dictionary<string, object>(defaultToken);
                    }
                }
            }

            return tokens;
        }

        /// <summary>
        /// Codifica los tokens en una secuencia de bytes comprimidos.
        /// </summary>
        public List<byte> EncodeTokens(List<Dictionary<string, object>> tokens)
        {
            Dictionary<string, object> writer = new Dictionary<string, object>()
            {
                {"output", new List<byte>()},
                {"control", (byte)0},
                {"bits", 8},
                {"buffer", new List<byte>()}
            };

            foreach (Dictionary<string, object> token in tokens)
            {
                if ((TokenType)token["type"] == TokenType.SHORT)
                {
                    int length = (((int)token["length"] - 2) << 4);
                    int distance = (int)token["offset"] - 1;
                    WriteCommandBit(writer, true, new List<byte>() { (byte)(0x80 | length | distance) });
                }
                else if ((TokenType)token["type"] == TokenType.LONG)
                {
                    int length = (((int)token["length"] - 3) << 10);
                    int distance = (int)token["offset"];
                    int composite = length | distance;
                    WriteCommandBit(writer, true, new List<byte>() { (byte)(composite >> 8), (byte)(composite & 0xFF) });
                }
                else if ((TokenType)token["type"] == TokenType.RAW)
                {
                    if (((List<byte>)token["data"]).Count < 8)
                    {
                        foreach (byte data in (List<byte>)token["data"])
                        {
                            WriteCommandBit(writer, false, new List<byte>() { data });
                        }
                    }
                    else
                    {
                        WriteCommandBit(writer, true, new List<byte>() { (byte)(0xB8 + ((List<byte>)token["data"]).Count) }.Concat((List<byte>)token["data"]).ToList());
                    }
                }
                else
                {
                    throw new Exception("Tokenizer Error");
                }
            }

            WriteCommandBit(writer, true, new List<byte>() { 0xFF });

            while ((int)writer["bits"] != 8)
            {
                WriteCommandBit(writer, true, new List<byte>());
            }

            return (List<byte>)writer["output"];
        }

        /// <summary>
        /// Guarda los datos comprimidos en un archivo.
        /// </summary>
        public void SaveData(List<byte> data, string output)
        {
            using (FileStream fileStream = new FileStream(output, FileMode.Create))
            {
                fileStream.Write(data.ToArray(), 0, data.Count);
            }
        }

        /// <summary>
        /// Proceso principal para guardar un archivo comprimido.
        /// </summary>
        public bool SaveFile(string inputFile, string outputFile)
        {
            try
            {
                List<byte> buffer;
                byte[] buffer_slice;

                buffer = File.ReadAllBytes(inputFile).ToList();
                ushort clut_bytes = BitConverter.ToUInt16(new byte[] { buffer[8], buffer[9] }, 0);
                int CLUT_SIZE = clut_bytes + 12;
                buffer_slice = buffer.Skip(8 + CLUT_SIZE).ToArray();

                List<Dictionary<string, object>> tokens = GetTokens(buffer, buffer_slice);
                List<byte> encoding = EncodeTokens(tokens);
                SaveData(encoding, outputFile);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
