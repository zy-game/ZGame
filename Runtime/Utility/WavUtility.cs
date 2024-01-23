using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ZGame
{
    public class WavUtility
    {
        private const int BlockSize_16Bit = 2;
        private const int k_SizeofInt16 = 2;

        public static AudioClip ToAudioClip(string filePath)
        {
            if (filePath.StartsWith(Application.persistentDataPath) || filePath.StartsWith(Application.dataPath))
                return WavUtility.ToAudioClip(File.ReadAllBytes(filePath));
            Debug.LogWarning((object)"This only supports files that are stored using Unity's Application data path. \nTo load bundled resources use 'Resources.Load(\"filename\") typeof(AudioClip)' method. \nhttps://docs.unity3d.com/ScriptReference/Resources.Load.html");
            return (AudioClip)null;
        }

        public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
        {
            int int32_1 = BitConverter.ToInt32(fileBytes, 16);
            ushort uint16_1 = BitConverter.ToUInt16(fileBytes, 20);
            string str = WavUtility.FormatCode(uint16_1);
            int num1;
            switch (uint16_1)
            {
                case 1:
                    num1 = 1;
                    break;
                case 65534:
                    num1 = 1;
                    break;
                default:
                    num1 = 0;
                    break;
            }

            Debug.AssertFormat(((uint)num1 > 0U ? 1 : 0) != 0, "Detected format code '{0}' {1}, but only PCM and WaveFormatExtensable uncompressed formats are currently supported.", (object)uint16_1, (object)str);
            ushort uint16_2 = BitConverter.ToUInt16(fileBytes, 22);
            int int32_2 = BitConverter.ToInt32(fileBytes, 24);
            ushort uint16_3 = BitConverter.ToUInt16(fileBytes, 34);
            int num2 = 20 + int32_1 + 4;
            int int32_3 = BitConverter.ToInt32(fileBytes, num2);
            float[] audioClipData;
            switch (uint16_3)
            {
                case 8:
                    audioClipData = WavUtility.Convert8BitByteArrayToAudioClipData(fileBytes, num2, int32_3);
                    break;
                case 16:
                    audioClipData = WavUtility.Convert16BitByteArrayToAudioClipData(fileBytes, num2, int32_3);
                    break;
                case 24:
                    audioClipData = WavUtility.Convert24BitByteArrayToAudioClipData(fileBytes, num2, int32_3);
                    break;
                case 32:
                    audioClipData = WavUtility.Convert32BitByteArrayToAudioClipData(fileBytes, num2, int32_3);
                    break;
                default:
                    throw new Exception(uint16_3.ToString() + " bit depth is not supported.");
            }

            AudioClip audioClip = AudioClip.Create(name, audioClipData.Length, (int)uint16_2, int32_2, false);
            audioClip.SetData(audioClipData, 0);
            return audioClip;
        }

        public static void ConvertAudioClipDataToInt16ByteArray(
            IReadOnlyList<float> input,
            int size,
            byte[] output)
        {
            MemoryStream memoryStream = new MemoryStream(output);
            for (int index = 0; index < size; ++index)
                memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(input[index] * (float)short.MaxValue)), 0, 2);
            memoryStream.Dispose();
        }

        private static float[] Convert8BitByteArrayToAudioClipData(
            byte[] source,
            int headerOffset,
            int dataSize)
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 > 0 && (int32 == dataSize || 0U > 0U) ? 1 : 0) != 0, "Failed to get valid 8-bit wav size: {0} from data bytes: {1} at offset: {2}", (object)int32, (object)dataSize, (object)headerOffset);
            float[] audioClipData = new float[int32];
            sbyte maxValue = sbyte.MaxValue;
            for (int index = 0; index < int32; ++index)
                audioClipData[index] = (float)source[index] / (float)maxValue;
            return audioClipData;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(
            byte[] source,
            int headerOffset,
            int dataSize)
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 > 0 && (int32 == dataSize || 0U > 0U) ? 1 : 0) != 0, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", (object)int32, (object)dataSize, (object)headerOffset);
            int num = 2;
            int length = int32 / num;
            float[] audioClipData = new float[length];
            short maxValue = short.MaxValue;
            for (int index = 0; index < length; ++index)
            {
                int startIndex = index * num + headerOffset;
                audioClipData[index] = (float)BitConverter.ToInt16(source, startIndex) / (float)maxValue;
            }

            Debug.AssertFormat((audioClipData.Length == length || 0U > 0U ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", (object)audioClipData.Length, (object)length);
            return audioClipData;
        }

        private static float[] Convert24BitByteArrayToAudioClipData(
            byte[] source,
            int headerOffset,
            int dataSize)
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 > 0 && (int32 == dataSize || 0U > 0U) ? 1 : 0) != 0, "Failed to get valid 24-bit wav size: {0} from data bytes: {1} at offset: {2}", (object)int32, (object)dataSize, (object)headerOffset);
            int count = 3;
            int length = int32 / count;
            int maxValue = int.MaxValue;
            float[] audioClipData = new float[length];
            byte[] dst = new byte[4];
            for (int index = 0; index < length; ++index)
            {
                int srcOffset = index * count + headerOffset;
                Buffer.BlockCopy((Array)source, srcOffset, (Array)dst, 1, count);
                audioClipData[index] = (float)BitConverter.ToInt32(dst, 0) / (float)maxValue;
            }

            Debug.AssertFormat((audioClipData.Length == length || 0U > 0U ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", (object)audioClipData.Length, (object)length);
            return audioClipData;
        }

        private static float[] Convert32BitByteArrayToAudioClipData(
            byte[] source,
            int headerOffset,
            int dataSize)
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 > 0 && (int32 == dataSize || 0U > 0U) ? 1 : 0) != 0, "Failed to get valid 32-bit wav size: {0} from data bytes: {1} at offset: {2}", (object)int32, (object)dataSize, (object)headerOffset);
            int num = 4;
            int length = int32 / num;
            int maxValue = int.MaxValue;
            float[] audioClipData = new float[length];
            for (int index = 0; index < length; ++index)
            {
                int startIndex = index * num + headerOffset;
                audioClipData[index] = (float)BitConverter.ToInt32(source, startIndex) / (float)maxValue;
            }

            Debug.AssertFormat((audioClipData.Length == length || 0U > 0U ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", (object)audioClipData.Length, (object)length);
            return audioClipData;
        }

        public static byte[] FromAudioClip(AudioClip audioClip) => WavUtility.FromAudioClip(audioClip, out string _, false);

        public static byte[] FromAudioClip(
            AudioClip audioClip,
            out string filepath,
            bool saveAsFile = true,
            string dirname = "recordings")
        {
            MemoryStream stream = new MemoryStream();
            ushort bitDepth = 16;
            int fileSize = audioClip.samples * 2 + 44;
            WavUtility.WriteFileHeader(ref stream, fileSize);
            WavUtility.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
            WavUtility.WriteFileData(ref stream, audioClip, bitDepth);
            byte[] array = stream.ToArray();
            Debug.AssertFormat((array.Length == fileSize || 0U > 0U ? 1 : 0) != 0, "Unexpected AudioClip to wav format byte count: {0} == {1}", (object)array.Length, (object)fileSize);
            if (saveAsFile)
            {
                filepath = string.Format("{0}/{1}/{2}.{3}", (object)Application.persistentDataPath, (object)dirname, (object)DateTime.UtcNow.ToString("yyMMdd-HHmmss-fff"), (object)"wav");
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                File.WriteAllBytes(filepath, array);
            }
            else
                filepath = (string)null;

            stream.Dispose();
            return array;
        }

        private static int WriteFileHeader(ref MemoryStream stream, int fileSize)
        {
            int num1 = 0;
            int num2 = 12;
            byte[] bytes1 = Encoding.ASCII.GetBytes("RIFF");
            int num3 = num1 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes1, "ID");
            int num4 = fileSize - 8;
            int num5 = num3 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num4), "CHUNK_SIZE");
            byte[] bytes2 = Encoding.ASCII.GetBytes("WAVE");
            int num6 = num5 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes2, "FORMAT");
            Debug.AssertFormat((num6 == num2 || 0U > 0U ? 1 : 0) != 0, "Unexpected wav descriptor byte count: {0} == {1}", (object)num6, (object)num2);
            return num6;
        }

        private static int WriteFileFormat(
            ref MemoryStream stream,
            int channels,
            int sampleRate,
            ushort bitDepth)
        {
            int num1 = 0;
            int num2 = 24;
            byte[] bytes = Encoding.ASCII.GetBytes("fmt ");
            int num3 = num1 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "FMT_ID");
            int num4 = 16;
            int num5 = num3 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num4), "SUBCHUNK_SIZE");
            ushort num6 = 1;
            int num7 = num5 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num6), "AUDIO_FORMAT");
            ushort uint16_1 = Convert.ToUInt16(channels);
            int num8 = num7 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_1), "CHANNELS") + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(sampleRate), "SAMPLE_RATE");
            int num9 = sampleRate * channels * WavUtility.BytesPerSample(bitDepth);
            int num10 = num8 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num9), "BYTE_RATE");
            ushort uint16_2 = Convert.ToUInt16(channels * WavUtility.BytesPerSample(bitDepth));
            int num11 = num10 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_2), "BLOCK_ALIGN") + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(bitDepth), "BITS_PER_SAMPLE");
            Debug.AssertFormat((num11 == num2 || 0U > 0U ? 1 : 0) != 0, "Unexpected wav fmt byte count: {0} == {1}", (object)num11, (object)num2);
            return num11;
        }

        private static int WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
        {
            int num1 = 0;
            int num2 = 8;
            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);
            byte[] int16ByteArray = WavUtility.ConvertAudioClipDataToInt16ByteArray(data);
            byte[] bytes = Encoding.ASCII.GetBytes("data");
            int num3 = num1 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "DATA_ID");
            int int32 = Convert.ToInt32(audioClip.samples * 2);
            int num4 = num3 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(int32), "SAMPLES");
            Debug.AssertFormat((num4 == num2 || 0U > 0U ? 1 : 0) != 0, "Unexpected wav data id byte count: {0} == {1}", (object)num4, (object)num2);
            int num5 = num4 + WavUtility.WriteBytesToMemoryStream(ref stream, int16ByteArray, "DATA");
            Debug.AssertFormat((int16ByteArray.Length == int32 || 0U > 0U ? 1 : 0) != 0, "Unexpected AudioClip to wav subchunk2 size: {0} == {1}", (object)int16ByteArray.Length, (object)int32);
            return num5;
        }

        public static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
        {
            MemoryStream memoryStream = new MemoryStream();
            int count = 2;
            short maxValue = short.MaxValue;
            for (int index = 0; index < data.Length; ++index)
                memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[index] * (float)maxValue)), 0, count);
            byte[] array = memoryStream.ToArray();
            Debug.AssertFormat((data.Length * count == array.Length || 0U > 0U ? 1 : 0) != 0, "Unexpected float[] to Int16 to byte[] size: {0} == {1}", (object)(data.Length * count), (object)array.Length);
            memoryStream.Dispose();
            return array;
        }

        private static int WriteBytesToMemoryStream(ref MemoryStream stream, byte[] bytes, string tag = "")
        {
            int length = bytes.Length;
            stream.Write(bytes, 0, length);
            return length;
        }

        public static ushort BitDepth(AudioClip audioClip)
        {
            ushort uint16 = Convert.ToUInt16((float)(audioClip.samples * audioClip.channels) * audioClip.length / (float)audioClip.frequency);
            int num;
            switch (uint16)
            {
                case 8:
                case 16:
                    num = 1;
                    break;
                default:
                    num = uint16 == (ushort)32 ? 1 : 0;
                    break;
            }

            object[] objArray = new object[1] { (object)uint16 };
            Debug.AssertFormat(num != 0, "Unexpected AudioClip bit depth: {0}. Expected 8 or 16 or 32 bit.", objArray);
            return uint16;
        }

        private static int BytesPerSample(ushort bitDepth) => (int)bitDepth / 8;

        private static int BlockSize(ushort bitDepth)
        {
            switch (bitDepth)
            {
                case 8:
                    return 1;
                case 16:
                    return 2;
                case 32:
                    return 4;
                default:
                    throw new Exception(bitDepth.ToString() + " bit depth is not supported.");
            }
        }

        private static string FormatCode(ushort code)
        {
            switch (code)
            {
                case 1:
                    return "PCM";
                case 2:
                    return "ADPCM";
                case 3:
                    return "IEEE";
                case 7:
                    return "μ-law";
                case 65534:
                    return "WaveFormatExtensable";
                default:
                    Debug.LogWarning((object)("Unknown wav code format:" + code.ToString()));
                    return "";
            }
        }
    }
}