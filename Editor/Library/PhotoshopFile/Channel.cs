using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace PhotoshopFile
{
	[DebuggerDisplay("ID = {ID}")]
	public class Channel
	{
		public Layer Layer { get; private set; }

		public short ID { get; set; }

		public Rect Rect
		{
			get
			{
				switch (ID)
				{
				case -2:
					return Layer.Masks.LayerMask.Rect;
				case -3:
					return Layer.Masks.UserMask.Rect;
				default:
					return Layer.Rect;
				}
			}
		}

		public int Length { get; set; }

		public byte[] ImageDataRaw { get; set; }

		public byte[] ImageData { get; set; }

		public ImageCompression ImageCompression { get; set; }

		public RleRowLengths RleRowLengths { get; set; }

		internal Channel(short id, Layer layer)
		{
			ID = id;
			Layer = layer;
		}

		internal Channel(PsdBinaryReader reader, Layer layer)
		{
			ID = reader.ReadInt16();
			Length = reader.ReadInt32();
			Layer = layer;
		}

		internal void Save(PsdBinaryWriter writer)
		{
			writer.Write(ID);
			writer.Write(Length);
		}

		internal void LoadPixelData(PsdBinaryReader reader)
		{
			long num = reader.BaseStream.Position + Length;
			ImageCompression = (ImageCompression)reader.ReadInt16();
			int count = Length - 2;
			switch (ImageCompression)
			{
			case ImageCompression.Raw:
				ImageDataRaw = reader.ReadBytes(count);
				break;
			case ImageCompression.Rle:
			{
				RleRowLengths = new RleRowLengths(reader, (int)Rect.height);
				int count2 = (int)(num - reader.BaseStream.Position);
				ImageDataRaw = reader.ReadBytes(count2);
				break;
			}
			case ImageCompression.Zip:
			case ImageCompression.ZipPrediction:
				ImageDataRaw = reader.ReadBytes(count);
				break;
			}
		}

		public void DecodeImageData()
		{
			if (ImageCompression == ImageCompression.Raw)
			{
				ImageData = ImageDataRaw;
			}
			else
			{
				DecompressImageData();
			}
			if (ImageCompression == ImageCompression.ZipPrediction)
			{
				UnpredictImageData(Rect);
			}
			else
			{
				ReverseEndianness(ImageData, Rect);
			}
		}

		private void DecompressImageData()
		{
			using (MemoryStream memoryStream = new MemoryStream(ImageDataRaw))
			{
				int num = Util.BytesPerRow(Rect, Layer.PsdFile.BitDepth);
				int num2 = (int)Rect.height * num;
				ImageData = new byte[num2];
				switch (ImageCompression)
				{
				case ImageCompression.Rle:
				{
					RleReader rleReader = new RleReader(memoryStream);
					for (int i = 0; (float)i < Rect.height; i++)
					{
						int offset = i * num;
						rleReader.Read(ImageData, offset, num);
					}
					break;
				}
				case ImageCompression.Zip:
				case ImageCompression.ZipPrediction:
				{
					memoryStream.ReadByte();
					memoryStream.ReadByte();
					DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
					int num3 = deflateStream.Read(ImageData, 0, num2);
					break;
				}
				default:
					throw new PsdInvalidException("Unknown image compression method.");
				}
			}
		}

		private void ReverseEndianness(byte[] buffer, Rect rect)
		{
			int num = Util.BytesFromBitDepth(Layer.PsdFile.BitDepth);
			int num2 = (int)(rect.width * rect.height);
			if (num2 != 0)
			{
				if (num == 2)
				{
					Util.SwapByteArray2(buffer, 0, num2);
				}
				else if (num == 4)
				{
					Util.SwapByteArray4(buffer, 0, num2);
				}
				else if (num > 1)
				{
					throw new NotImplementedException("Byte-swapping implemented only for 16-bit and 32-bit depths.");
				}
			}
		}

		private unsafe void UnpredictImageData(Rect rect)
		{
			if (Layer.PsdFile.BitDepth == 16)
			{
				ReverseEndianness(ImageData, rect);
				fixed (byte* ptr = &ImageData[0])
				{
					for (int i = 0; (float)i < rect.height; i++)
					{
						ushort* ptr2 = (ushort*)(ptr + (long)(i * (int)rect.width) * 2L);
						ushort* ptr3 = (ushort*)(ptr + (long)((i + 1) * (int)rect.width) * 2L);
						for (ptr2++; ptr2 < ptr3; ptr2++)
						{
							*ptr2 += *(ptr2 - 1);
						}
					}
				}
				return;
			}
			if (Layer.PsdFile.BitDepth == 32)
			{
				byte[] array = new byte[ImageData.Length];
				fixed (byte* ptr = &ImageData[0])
				{
					for (int i = 0; (float)i < rect.height; i++)
					{
						byte* ptr4 = ptr + (long)(i * (int)rect.width) * 4L;
						byte* ptr5 = ptr + (long)((i + 1) * (int)rect.width) * 4L;
						for (ptr4++; ptr4 < ptr5; ptr4++)
						{
							*ptr4 += *(ptr4 - 1);
						}
					}
					int num = (int)rect.width;
					int num2 = 2 * num;
					int num3 = 3 * num;
					fixed (byte* ptr6 = &array[0])
					{
						for (int i = 0; (float)i < rect.height; i++)
						{
							byte* ptr7 = ptr6 + (long)(i * (int)rect.width) * 4L;
							byte* ptr8 = ptr6 + (long)((i + 1) * (int)rect.width) * 4L;
							byte* ptr9 = ptr + (long)(i * (int)rect.width) * 4L;
							while (ptr7 < ptr8)
							{
								*(ptr7++) = ptr9[num3];
								*(ptr7++) = ptr9[num2];
								*(ptr7++) = ptr9[num];
								*(ptr7++) = *ptr9;
								ptr9++;
							}
						}
					}
				}
				ImageData = array;
				return;
			}
			throw new PsdInvalidException("ZIP with prediction is only available for 16 and 32 bit depths.");
		}

		public void CompressImageData()
		{
			if (ImageDataRaw != null || ImageData == null)
			{
				return;
			}
			if (ImageCompression == ImageCompression.Raw)
			{
				ImageDataRaw = ImageData;
				Length = 2 + ImageDataRaw.Length;
				return;
			}
			if (ImageCompression == ImageCompression.Rle)
			{
				RleRowLengths = new RleRowLengths((int)Rect.height);
				using (MemoryStream memoryStream = new MemoryStream())
				{
					RleWriter rleWriter = new RleWriter(memoryStream);
					int count = Util.BytesPerRow(Rect, Layer.PsdFile.BitDepth);
					for (int i = 0; (float)i < Rect.height; i++)
					{
						int offset = i * (int)Rect.width;
						RleRowLengths[i] = rleWriter.Write(ImageData, offset, count);
					}
					memoryStream.Flush();
					ImageDataRaw = memoryStream.ToArray();
				}
				Length = 2 + 2 * (int)Rect.height + ImageDataRaw.Length;
				return;
			}
			throw new NotImplementedException("Only raw and RLE compression have been implemented.");
		}

		internal void SavePixelData(PsdBinaryWriter writer)
		{
			writer.Write((short)ImageCompression);
			if (ImageDataRaw != null)
			{
				if (ImageCompression == ImageCompression.Rle)
				{
					RleRowLengths.Write(writer);
				}
				writer.Write(ImageDataRaw);
			}
		}
	}
}
