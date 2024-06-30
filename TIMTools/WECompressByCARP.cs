using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIMTools
{
    public  class WECompressByCARP
    {
        private const uint BUFFER_MASK = 0x3FF;


        private const int BufferSize = 0x400;
        private const int MinMatchLength = 2;
        private const int MaxMatchLength = 34;
        private const int ShortDistanceLimit = 8;
        private const int LongDistanceLimit = 1024;

        private byte[] inputBuffer;
        private int inputOffset;

        private byte[] outputBuffer;
        private int outputOffset;

        private byte[] slidingWindow;
        private int slidingWindowOffset;



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

        public void SaveData(List<byte> data, string output)
        {
            byte[] buffer;
            byte[] ajustaBuffer = new byte[1];
            double resto;

            using (FileStream archivo = new FileStream(output, FileMode.Create))
            {
                buffer = data.ToArray();
                int largoBuffer = buffer.Length;

                // Setting the correct length of the BIN
                double largoArchivo = ((double)buffer.Length + 1) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);

                if ((resto == 0) || (resto == 0.25) || (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 1];

                // Largo + 0000
                largoArchivo = ((double)buffer.Length + 2) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);

                if ((resto == 0) || (resto == 0.25) || (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 2];

                // Largo + 000000
                largoArchivo = ((double)buffer.Length + 3) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);

                if ((resto == 0) || (resto == 0.25) || (resto == 0.50) || (resto == 0.75))
                    ajustaBuffer = new byte[largoBuffer + 3];

                // Largo + 00000000
                largoArchivo = ((double)buffer.Length + 4) / 16;
                resto = largoArchivo - Math.Floor(largoArchivo);

                if ((resto == 0) || (resto == 0.25) || (resto == 0.50) || (resto == 0.75))
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

                buffer = File.ReadAllBytes(filePath).ToList();
                ushort clut_bytes = ushort.Parse($"{buffer[8]:X2}{buffer[9]:X2}", System.Globalization.NumberStyles.HexNumber);
                byte[] clut_bytes_array = BitConverter.GetBytes(clut_bytes);
                clut_bytes = BitConverter.ToUInt16(clut_bytes_array.Reverse().ToArray(), 0);
                int CLUT_SIZE = clut_bytes + 12;
                buffer_slice = buffer.Skip(8 + CLUT_SIZE).ToArray();

                List<Dictionary<string, object>> tokens = GetTokens(buffer, buffer_slice);
                List<byte> encoding = EncodeTokens(tokens);
                SaveData(encoding, outputFile);
                result = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public byte[] DeCompress(byte[] originalGraphics)
        {
            List<byte> graphicsModified = new List<byte>();
            int i = 0;
            int counter = 0;
            int modif = 0;
            int orig = 1;

            while (true)
            {
                bool lLaco = false;

                if (i < 256)
                {
                    int k = originalGraphics[orig];
                    orig++;
                    counter++;
                    i = k + 65280;
                }

                int k2 = originalGraphics[orig];
                if (i % 2 == 0)
                {
                    graphicsModified.Add((byte)k2);
                    modif++;
                    orig++;
                    counter++;
                }
                else
                {
                    if ((k2 > 128) != false)
                    {
                        orig++;
                        counter++;
                        if ((k2 > 192) != false)
                        {
                            int k = k2;
                            int k3 = k - 185;
                            if (k == 255) break;

                            do
                            {
                                k2 = originalGraphics[orig];
                                orig++;
                                counter++;
                                graphicsModified[modif - 2] = (byte)k2;
                                modif++;
                                k3--;
                            } while (k3 > 0 || k3 == 0);

                            lLaco = true;
                        }

                        if (!lLaco)
                        {
                            int j = (k2 & 15) + 1;
                            int k3 = (k2 / 16) - 7;

                            do
                            {
                                k3--;
                                graphicsModified.Add(graphicsModified[modif - j]);
                                modif++;
                            } while (k3 > 0 || k3 == 0);
                        }
                    }
                    else
                    {
                        int j = originalGraphics[orig + 1];
                        orig += 2;
                        counter += 2;
                        int k3 = (k2 / 4) + 2;
                        j |= (k2 & 3) * 256;

                        do
                        {
                            k3--;
                            graphicsModified.Add(graphicsModified[modif - j]);
                            modif++;
                        } while (k3 > 0 || k3 == 0);
                    }
                }

                i /= 2;
            }

            return graphicsModified.ToArray();
        }

  

        
        /// <summary>
        /// FUNCIONA PERFECTO!!!!!
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcSize"></param>
        /// <param name="dst"></param>
        /// <param name="dstSize"></param>
        /// <returns></returns>
        public int Decompress(byte[] src, int srcSize,ref byte[] dst, int dstSize)
        {
            int srcOffset = 0;
            int dstOffset = 0;
            //byte[] srcTemp = new byte[src.Length];
            byte[] buffer = new byte[0x400];
            int bufferOffset = 0;
            List<byte> output = new List<byte>();   
            byte data = 0;
            uint control = 0;
            int length = 0;
            uint distance = 0;
            int loop = 0;

            while (true)
            {
                loop = 0;

                control >>= 1;
                if (control < 0x100)
                {
                    control = (uint)(src[srcOffset++] | 0xFF00);
                    //Console.WriteLine($"control={control:X8}");
                }

                data = src[srcOffset++];

                // direct copy
                // can do stream of 1 - 8 direct copies
                if ((control & 1) == 0)
                {
                    //Console.WriteLine($"{dstOffset:X8}: direct copy {data:X2}");
                    output.Add(data);
                    //dst[dstOffset++] = data;
                    buffer[bufferOffset] = data;
                    bufferOffset = (bufferOffset + 1) & (int)BUFFER_MASK;
                    continue;
                }

                // window copy (long distance)
                if ((data & 0x80) == 0)
                {
                    /*
                    input stream:
                    00: 0bbb bbaa
                    01: dddd dddd
                    distance: [0 - 1023] (00aa dddd dddd)
                    length: [2 - 33] (000b bbbb)
                    */
                    distance = (uint)(src[srcOffset++] | ((data & 0x3) << 8));
                    length = (data >> 2) + 2;
                    loop = 1;
                    //Console.WriteLine($"long distance: distance={distance:X8} length={length:X8} data={data:X2}");
                    //Console.WriteLine($"{dstOffset:X8}: window copy (long): {length} bytes from {dstOffset - distance:X8}");
                }

                // window copy (short distance)
                else if ((data & 0x40) == 0)
                {
                    /*
                    input stream:
                    00: llll dddd
                    distance: [1 - 16] (dddd)
                    length: [1 - 4] (llll)
                    */
                    distance = (uint)(data & 0xF) + 1;
                    length = (data >> 4) - 7;
                    loop = 1;
                    //Console.WriteLine($"short distance: distance={distance:X8} length={length:X8} data={data:X2}");
                    //Console.WriteLine($"{dstOffset:X8}: window copy (short): {length} bytes from {dstOffset - distance:X8}");
                }

                if (loop != 0)
                {
                    // copy length bytes from window
                    while (length-- >= 0)
                    {
                        data = buffer[(bufferOffset - (int)distance) & (int)BUFFER_MASK];
                        output.Add(data);
                        //dst[dstOffset++] = data;
                        buffer[bufferOffset] = data;
                        bufferOffset = (bufferOffset + 1) & (int)BUFFER_MASK;
                    }
                    continue;
                }

                // end of stream
                if (data == 0xFF)
                    break;

                // block copy
                // directly copy group of bytes
                /*
                input stream:
                00: llll lll0
                length: [8 - 69]
                directly copy (length+1) bytes
                */
                length = data - 0xB9;
                //Console.WriteLine($"block copy {length + 1} bytes");
                while (length-- >= 0)
                {
                    data = src[srcOffset++];
                    output.Add(data);
                    //dst[dstOffset++] = data;
                    buffer[bufferOffset] = data;
                    bufferOffset = (bufferOffset + 1) & (int)BUFFER_MASK;
                }
            }
            dst = output.ToArray();
            return dstOffset;
        }


        public byte[] Compress(byte[] data)
        {
            inputBuffer = data;
            inputOffset = 0;
            outputBuffer = new byte[data.Length * 2]; // Estimation of compressed size
            outputOffset = 0;
            slidingWindow = new byte[BufferSize];
            slidingWindowOffset = 0;

            while (inputOffset < data.Length)
            {
                int bestLength = 0;
                int bestDistance = 0;

                for (int distance = 1; distance <= Math.Min(ShortDistanceLimit, slidingWindowOffset); distance++)
                {
                    int length = FindMatchLength(distance);
                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestDistance = distance;
                    }
                }

                if (bestLength >= MinMatchLength)
                {
                    EmitReference(bestLength, bestDistance);
                }
                else
                {
                    EmitLiteral();
                }
            }

            // End of stream marker
            outputBuffer[outputOffset++] = 0xFF;

            Array.Resize(ref outputBuffer, outputOffset);
            return outputBuffer;
        }

        private int FindMatchLength(int distance)
        {
            int maxLength = Math.Min(MaxMatchLength, inputBuffer.Length - inputOffset);
            int matchLength = 0;

            for (int i = 0; i < maxLength; i++)
            {
                if (inputBuffer[inputOffset - distance + i] == inputBuffer[inputOffset + i])
                {
                    matchLength++;
                }
                else
                {
                    break;
                }
            }

            return matchLength;
        }

        private void EmitLiteral()
        {
            outputBuffer[outputOffset++] = inputBuffer[inputOffset];
            slidingWindow[slidingWindowOffset] = inputBuffer[inputOffset];
            slidingWindowOffset = (slidingWindowOffset + 1) % BufferSize;
            inputOffset++;
        }

        private void EmitReference(int length, int distance)
        {
            byte controlByte = (byte)(((length - MinMatchLength) << 2) | ((distance >> 8) & 0x03));
            byte distanceByte = (byte)(distance & 0xFF);

            outputBuffer[outputOffset++] = controlByte;
            outputBuffer[outputOffset++] = distanceByte;

            for (int i = 0; i < length; i++)
            {
                outputBuffer[outputOffset++] = inputBuffer[inputOffset];
                slidingWindow[slidingWindowOffset] = inputBuffer[inputOffset];
                slidingWindowOffset = (slidingWindowOffset + 1) % BufferSize;
                inputOffset++;
            }
        }

        public byte[] Decompress(byte[] data, int decompressedSize)
        {
            inputBuffer = data;
            inputOffset = 0;
            outputBuffer = new byte[decompressedSize];
            outputOffset = 0;
            slidingWindow = new byte[BufferSize];
            slidingWindowOffset = 0;

            while (outputOffset < decompressedSize)
            {
                byte control = inputBuffer[inputOffset++];

                if (control == 0xFF)
                {
                    break; // End of stream marker
                }

                if ((control & 0x01) == 0)
                {
                    // Direct copy
                    byte dataByte = inputBuffer[inputOffset++];
                    outputBuffer[outputOffset++] = dataByte;
                    slidingWindow[slidingWindowOffset] = dataByte;
                    slidingWindowOffset = (slidingWindowOffset + 1) % BufferSize;
                }
                else
                {
                    // Window copy
                    int length;
                    int distance;

                    if ((control & 0x80) == 0)
                    {
                        // Long distance
                        distance = (((control & 0x03) << 8) | inputBuffer[inputOffset++]) + 1;
                        length = (control >> 2) + 2;
                    }
                    else if ((control & 0x40) == 0)
                    {
                        // Short distance
                        distance = ((control & 0x0F) << 8 | inputBuffer[inputOffset++]) + 1;
                        length = (control >> 4) + 3;
                    }
                    else
                    {
                        // Block copy
                        length = control - 0xB8;
                        for (int i = 0; i <= length; i++)
                        {
                            byte dataByte = inputBuffer[inputOffset++];
                            outputBuffer[outputOffset++] = dataByte;
                            slidingWindow[slidingWindowOffset] = dataByte;
                            slidingWindowOffset = (slidingWindowOffset + 1) % BufferSize;
                        }
                        continue;
                    }

                    for (int i = 0; i < length; i++)
                    {
                        byte dataByte = slidingWindow[(slidingWindowOffset - distance + i) % BufferSize];
                        outputBuffer[outputOffset++] = dataByte;
                        slidingWindow[slidingWindowOffset] = dataByte;
                        slidingWindowOffset = (slidingWindowOffset + 1) % BufferSize;
                    }
                }
            }

            return outputBuffer;
        }

    }
}
