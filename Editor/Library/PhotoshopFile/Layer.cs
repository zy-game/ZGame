using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.U2D;

namespace PhotoshopFile
{
    /// <summary>
    /// Represents the possible justification settings for text.
    /// </summary>
    public enum TextJustification
    {
        /// <summary>
        /// The text is left justified.
        /// </summary>
        Left,

        /// <summary>
        /// The text is right justified.
        /// </summary>
        Right,

        /// <summary>
        /// The text is center justified.
        /// </summary>
        Center
    }
    [DebuggerDisplay("Name = {Name}")]
    public class Layer
    {
        private string blendModeKey;

        private static int protectTransBit = BitVector32.CreateMask();

        private static int visibleBit = BitVector32.CreateMask(protectTransBit);

        private BitVector32 flags = default(BitVector32);

        internal PsdFile PsdFile { get; private set; }

        public Rect Rect { get; set; }

        public ChannelList Channels { get; private set; }

        public bool IsGroup { get; set; }

        public bool IsEndGroupMarker { get; set; }

        public Channel AlphaChannel
        {
            get
            {
                if (Channels.ContainsId(-1))
                {
                    return Channels.GetId(-1);
                }
                return null;
            }
        }
        public Layer Parent { get; set; }
        public List<Layer> Chiled { get; set; }
        public Texture2D texture { get; private set; }
        public string BlendModeKey
        {
            get
            {
                return blendModeKey;
            }
            set
            {
                if (value.Length != 4)
                {
                    throw new ArgumentException("Key length must be 4");
                }
                blendModeKey = value;
            }
        }

        public byte Opacity { get; set; }

        public bool Clipping { get; set; }

        public bool Visible
        {
            get
            {
                return !flags[visibleBit];
            }
            set
            {
                flags[visibleBit] = !value;
            }
        }

        public bool ProtectTrans
        {
            get
            {
                return flags[protectTransBit];
            }
            set
            {
                flags[protectTransBit] = value;
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public BlendingRanges BlendingRangesData { get; set; }

        public MaskInfo Masks { get; set; }

        public List<LayerInfo> AdditionalInfo { get; set; }




        /// <summary>
        /// Gets a value indicating whether this layer is a text layer.
        /// </summary>
        public bool IsTextLayer { get; private set; }

        /// <summary>
        /// Gets the actual text string, if this is a text layer.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the point size of the font, if this is a text layer.
        /// </summary>
        public float FontSize { get; private set; }

        /// <summary>
        /// Gets the name of the font used, if this is a text layer.
        /// </summary>
        public string FontName { get; private set; }

        /// <summary>
        /// Gets the justification of the text, if this is a text layer.
        /// </summary>
        public TextJustification Justification { get; private set; }

        /// <summary>
        /// Gets the Fill Color of the text, if this is a text layer.
        /// </summary>
        public Color FillColor { get; private set; }

        /// <summary>
        /// Gets the style of warp done on the text, if it is a text layer.
        /// Can be warpNone, warpTwist, etc.
        /// </summary>
        public string WarpStyle { get; private set; }


        public Layer(PsdFile psdFile)
        {
            PsdFile = psdFile;
            Rect = default(Rect);
            Channels = new ChannelList();
            BlendModeKey = "norm";
            AdditionalInfo = new List<LayerInfo>();
            Chiled = new List<Layer>();
        }

        public Layer(PsdBinaryReader reader, PsdFile psdFile) : this(psdFile)
        {
            Rect = reader.ReadRectangle();
            int num = reader.ReadUInt16();
            for (int i = 0; i < num; i++)
            {
                Channel item = new Channel(reader, this);
                Channels.Add(item);
            }
            string text = reader.ReadAsciiChars(4);
            if (text != "8BIM")
            {
                throw new PsdInvalidException("Invalid signature in layer header.");
            }
            BlendModeKey = reader.ReadAsciiChars(4);
            Opacity = reader.ReadByte();
            Clipping = reader.ReadBoolean();
            byte data = reader.ReadByte();
            flags = new BitVector32(data);
            reader.ReadByte();
            uint num2 = reader.ReadUInt32();
            long position = reader.BaseStream.Position;
            Masks = new MaskInfo(reader, this);
            BlendingRangesData = new BlendingRanges(reader, this);
            Name = reader.ReadPascalString(4);
            long num3 = position + num2;
            while (reader.BaseStream.Position < num3)
            {
                LayerInfo item2 = LayerInfoFactory.Load(reader);
                AdditionalInfo.Add(item2);
            }
            foreach (LayerInfo item3 in AdditionalInfo)
            {
                string key = item3.Key;
                if (key != null && key == "luni")
                {
                    Name = ((LayerUnicodeName)item3).Name;
                }
                if (key != null && key == "lyid")
                {
                    Id = (int)BitConverter.ToInt32(((RawLayerInfo)item3).Data);
                }
                if (key == "TySh")
                {
                    RawLayerInfo rawLayerInfo = item3 as RawLayerInfo;
                    ReadTextLayer(new BinaryReverseReader(new MemoryStream(rawLayerInfo.Data)));
                }
            }
        }


        /// <summary>
        /// Reads the text information for the layer.
        /// </summary>
        /// <param name="dataReader">The reader to use to read the text data.</param>
        private void ReadTextLayer(BinaryReverseReader dataReader)
        {
            IsTextLayer = true;

            // read the text layer's text string
            dataReader.Seek("/Text");
            dataReader.ReadBytes(4);
            Text = dataReader.ReadString();

            // read the text justification
            dataReader.Seek("/Justification ");
            int justification = dataReader.ReadByte() - 48;
            Justification = TextJustification.Left;
            if (justification == 1)
            {
                Justification = TextJustification.Right;
            }
            else if (justification == 2)
            {
                Justification = TextJustification.Center;
            }

            // read the font size
            dataReader.Seek("/FontSize ");
            FontSize = dataReader.ReadFloat();

            // read the font fill color
            dataReader.Seek("/FillColor");
            dataReader.Seek("/Values [ ");
            float alpha = dataReader.ReadFloat();
            dataReader.ReadByte();
            float red = dataReader.ReadFloat();
            dataReader.ReadByte();
            float green = dataReader.ReadFloat();
            dataReader.ReadByte();
            float blue = dataReader.ReadFloat();
            FillColor = new Color(red /** byte.MaxValue*/, green /** byte.MaxValue*/, blue /** byte.MaxValue*/, alpha /** byte.MaxValue*/);

            // read the font name
            dataReader.Seek("/FontSet ");
            dataReader.Seek("/Name");
            dataReader.ReadBytes(4);
            FontName = dataReader.ReadString();

            // read the warp style
            dataReader.Seek("warpStyle");
            dataReader.Seek("warpStyle");
            dataReader.ReadBytes(3);
            int num13 = dataReader.ReadByte();
            WarpStyle = string.Empty;

            for (; num13 > 0; --num13)
            {
                string str = WarpStyle + dataReader.ReadChar();
                WarpStyle = str;
            }
        }
        public unsafe void CreateMissingChannels()
        {
            short num = PsdFile.ColorMode.MinChannelCount();
            for (short num2 = 0; num2 < num; num2 = (short)(num2 + 1))
            {
                if (!Channels.ContainsId(num2))
                {
                    int num3 = (int)(Rect.height * Rect.width);
                    Channel channel = new Channel(num2, this);
                    channel.ImageData = new byte[num3];
                    fixed (byte* ptr = &channel.ImageData[0])
                    {
                        Util.Fill(ptr, ptr + num3, byte.MaxValue);
                    }
                    Channels.Add(channel);
                }
            }
        }

        public void PrepareSave()
        {
            foreach (Channel channel in Channels)
            {
                channel.CompressImageData();
            }
            IEnumerable<LayerInfo> source = AdditionalInfo.Where((LayerInfo x) => x is LayerUnicodeName);
            if (source.Count() > 1)
            {
                throw new PsdInvalidException("Layer has more than one LayerUnicodeName.");
            }
            LayerUnicodeName layerUnicodeName = (LayerUnicodeName)source.FirstOrDefault();
            if (layerUnicodeName == null)
            {
                layerUnicodeName = new LayerUnicodeName(Name);
                AdditionalInfo.Add(layerUnicodeName);
            }
            else if (layerUnicodeName.Name != Name)
            {
                layerUnicodeName.Name = Name;
            }
        }

        public void Save(PsdBinaryWriter writer)
        {
            writer.Write(Rect);
            writer.Write((short)Channels.Count);
            foreach (Channel channel in Channels)
            {
                channel.Save(writer);
            }
            writer.WriteAsciiChars("8BIM");
            writer.WriteAsciiChars(BlendModeKey);
            writer.Write(Opacity);
            writer.Write(Clipping);
            writer.Write((byte)flags.Data);
            writer.Write((byte)0);
            using (new PsdBlockLengthWriter(writer))
            {
                Masks.Save(writer);
                BlendingRangesData.Save(writer);
                long position = writer.BaseStream.Position;
                writer.WritePascalString(Name, 4, 31);
                foreach (LayerInfo item in AdditionalInfo)
                {
                    item.Save(writer);
                }
            }
        }

        public bool HasTextureLayer()
        {
            if (IsTextLayer || Rect.width <= 0 || Rect.height <= 0)
            {
                return false;
            }
            return true;
        }

        public bool HasLayer()
        {
            return Rect.width > 0 && Rect.height > 0;
        }

        /// <summary>
        /// Decodes a <see cref="Layer"/> into a <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="layer">The <see cref="Layer"/> to decode.</param>
        /// <returns>The <see cref="Texture2D"/> decoded from the layer.</returns>
        public Texture2D DecodeImage(float width = 0, float height = 0)
        {
            width = width <= 0 ? Rect.width : width;
            height = height <= 0 ? Rect.height : height;
            if (width == 0 || height == 0)
            {
                return default;
            }
            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);

            Color32[] colors = new Color32[(int)(width * height)];

            for (int y = 0; y < height; ++y)
            {
                int layerRow = y * (int)width;

                // we need to reverse the Y position for the Unity texture
                int textureRow = ((int)height - 1 - y) * (int)width;


                for (int x = 0; x < width; ++x)
                {
                    int layerPosition = layerRow + x;
                    int texturePosition = textureRow + x;

                    colors[texturePosition] = GetColor(layerPosition);

                    // set the alpha
                    if (Channels.ContainsId(-2))
                    {
                        byte color = GetColor(Masks.LayerMask, x, y);
                        colors[texturePosition].a = (byte)(colors[texturePosition].a * color);
                    }
                }
            }

            texture.SetPixels32(colors);
            return texture;
        }
        public Texture2D CreateTexture(int width = 0, int height = 0)
        {
            if (texture is not null)
            {
                return texture;
            }
            width = width <= 0 ? (int)Rect.width : width;
            height = height <= 0 ? (int)Rect.height : height;
            if (width == 0 || height == 0)
            {
                return default;
            }

            texture = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color32[] pixels = new Color32[width * height];

            Channel red = Channels.GetId(0);
            Channel green = Channels.GetId(1);
            Channel blue = Channels.GetId(2);
            Channel alpha = Channels.ContainsId(-1) ? Channels.GetId(-1) : null;

            for (int i = 0; i < pixels.Length; i++)
            {
                int position = (int)(i / width * Rect.width + i % width);
                byte r = red.ImageData[position];
                byte g = green.ImageData[position];
                byte b = blue.ImageData[position];
                byte a = 255;

                if (alpha != null)
                    a = alpha.ImageData[position];

                int mod = i % texture.width;
                int n = ((texture.width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        public Texture2D CreateCellTexture(Layer layer)
        {
            if ((int)layer.Rect.width == 0 || (int)layer.Rect.height == 0)
                return null;


            int width = (int)(layer.Rect.width > 50 ? 50 : layer.Rect.width);
            int height = (int)(layer.Rect.height > 50 ? 50 : layer.Rect.height);

            if (width == 0 || height == 0)
            {
                return default;
            }
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color32[] pixels = new Color32[width * height];

            width = width - width % 2;
            height = height - height % 2;
            Channel red = Channels.GetId(0);
            Channel green = Channels.GetId(1);
            Channel blue = Channels.GetId(2);
            Channel alpha = Channels.GetId(-1);
            int position = 0;
            int max_width = 0;
            int max_height = 0;
            int split_width = width / 2;
            int split_height = height / 2;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (i % width >= split_width)
                {
                    max_width = (int)(layer.Rect.width - split_width + i % split_width);
                }
                else
                {
                    max_width = (int)(i % split_width);
                }
                if (i / width >= split_height)
                {
                    max_height = (int)(layer.Rect.height - split_height + (i / width - split_height));
                }
                else
                {
                    max_height = (int)(i / width);
                }

                position = (int)(max_width + max_height * layer.Rect.width);
                if (position >= layer.Rect.width * layer.Rect.height)
                {
                    position = (int)(layer.Rect.width * layer.Rect.height - 1);
                }
                byte r = red.ImageData[position];
                byte g = green.ImageData[position];
                byte b = blue.ImageData[position];
                byte a = 255;

                if (alpha != null)
                    a = alpha.ImageData[position];

                int mod = i % tex.width;
                int n = ((tex.width - mod - 1) + i) - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }
        /// <summary>
        /// Gets the color at the given position in the given <see cref="Layer"/>.
        /// </summary>
        /// <param name="layer">The <see cref="Layer"/> to sample.</param>
        /// <param name="position">The position to sample.</param>
        /// <returns>The sampled color.</returns>
        private Color32 GetColor(int position)
        {
            Color32 baseColor = new Color32(1, 1, 1, 1);
            switch (PsdFile.ColorMode)
            {
                case PsdColorMode.Grayscale:
                case PsdColorMode.Duotone:
                    baseColor = new Color32(Channels.GetId(0).ImageData[position], Channels.GetId(0).ImageData[position], Channels.GetId(0).ImageData[position], 1);
                    break;
                case PsdColorMode.Indexed:
                    int index = Channels.GetId(0).ImageData[position];
                    baseColor = new Color32(PsdFile.ColorModeData[index], PsdFile.ColorModeData[index + 256], PsdFile.ColorModeData[index + 512], 1);
                    break;
                case PsdColorMode.RGB:
                    baseColor = new Color32(Channels.GetId(0).ImageData[position], Channels.GetId(1).ImageData[position], Channels.GetId(2).ImageData[position], 1);
                    break;
                case PsdColorMode.CMYK:
                    baseColor = CMYKToRGB(Channels.GetId(0).ImageData[position], Channels.GetId(1).ImageData[position], Channels.GetId(2).ImageData[position], Channels.GetId(3).ImageData[position]);
                    break;
                case PsdColorMode.Multichannel:
                    baseColor = CMYKToRGB(Channels.GetId(0).ImageData[position], Channels.GetId(1).ImageData[position], Channels.GetId(2).ImageData[position], 0);
                    break;
                case PsdColorMode.Lab:
                    baseColor = LabToRGB(Channels.GetId(0).ImageData[position], Channels.GetId(1).ImageData[position], Channels.GetId(2).ImageData[position]);
                    break;
            }

            // set the alpha
            if (AlphaChannel != null)
            {
                baseColor.a = AlphaChannel.ImageData[position];
            }

            return baseColor;
        }

        /// <summary>
        /// Gets the color at the given pixel position in the given mask.
        /// </summary>
        /// <param name="mask">The mask to sample.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>The mask color.</returns>
        private byte GetColor(Mask mask, int x, int y)
        {
            byte num = byte.MaxValue;
            if (mask.PositionVsLayer)
            {
                x -= (int)mask.Rect.x;
                y -= (int)mask.Rect.y;
            }
            else
            {
                x = x + (int)mask.Layer.Rect.x - (int)mask.Rect.x;
                y = y + (int)mask.Layer.Rect.y - (int)mask.Rect.y;
            }

            if (y >= 0 && (y < mask.Rect.height && x >= 0) && x < mask.Rect.width)
            {
                int index = (y * (int)mask.Rect.width) + x;
                num = index >= mask.ImageData.Length ? byte.MaxValue : mask.ImageData[index];
            }

            return num;
        }

        /// <summary>
        /// Converts Lab color to RGB color.
        /// </summary>
        /// <param name="lb">The lb channel.</param>
        /// <param name="ab">The ab channel.</param>
        /// <param name="bb">The bb channel.</param>
        /// <returns>The RGB color.</returns>
        private Color32 LabToRGB(byte lb, byte ab, byte bb)
        {
            double num1 = lb;
            double num2 = ab;
            double num3 = bb;
            double num4 = 2.56;
            double num5 = 1.0;
            double num6 = 1.0;
            int num7 = (int)(num1 / num4);
            int num8 = (int)((num2 / num5) - 128.0);
            int num9 = (int)((num3 / num6) - 128.0);
            double x1 = (num7 + 16.0) / 116.0;
            double x2 = (num8 / 500.0) + x1;
            double x3 = x1 - (num9 / 200.0);
            double num10 = Math.Pow(x1, 3.0) <= 0.008856 ? (x1 - 0.0) / 7.787 : Math.Pow(x1, 3.0);
            double num11 = Math.Pow(x2, 3.0) <= 0.008856 ? (x2 - 0.0) / 7.787 : Math.Pow(x2, 3.0);
            double num12 = Math.Pow(x3, 3.0) <= 0.008856 ? (x3 - 0.0) / 7.787 : Math.Pow(x3, 3.0);
            return XYZToRGB(95.047 * num11, 100.0 * num10, 108.883 * num12);
        }

        /// <summary>
        /// Converts XYZ color to RGB color.
        /// </summary>
        /// <param name="x">The x channel.</param>
        /// <param name="y">The y channel.</param>
        /// <param name="z">The z channel.</param>
        /// <returns>The RGB color.</returns>
        private Color32 XYZToRGB(double x, double y, double z)
        {
            double num1 = x / 100.0;
            double num2 = y / 100.0;
            double num3 = z / 100.0;

            double x1 = (num1 * 3.2406) + (num2 * -1.5372) + (num3 * -0.4986);
            double x2 = (num1 * -0.9689) + (num2 * 1.8758) + (num3 * 0.0415);
            double x3 = (num1 * 0.0557) + (num2 * -0.204) + (num3 * 1.057);

            double num4 = x1 <= 0.0031308 ? 12.92 * x1 : (1.055 * Math.Pow(x1, 5.0 / 12.0)) - 0.055;
            double num5 = x2 <= 0.0031308 ? 12.92 * x2 : (1.055 * Math.Pow(x2, 5.0 / 12.0)) - 0.055;
            double num6 = x3 <= 0.0031308 ? 12.92 * x3 : (1.055 * Math.Pow(x3, 5.0 / 12.0)) - 0.055;

            byte red = (byte)(num4 * 256.0);
            byte green = (byte)(num5 * 256.0);
            byte blue = (byte)(num6 * 256.0);

            if (red > byte.MaxValue)
            {
                red = byte.MaxValue;
            }

            if (green > byte.MaxValue)
            {
                green = byte.MaxValue;
            }

            if (blue > byte.MaxValue)
            {
                blue = byte.MaxValue;
            }

            return new Color32(red, green, blue, 1);
        }

        /// <summary>
        /// Converts CMYK color to RGB color.
        /// </summary>
        /// <param name="c">The c channel.</param>
        /// <param name="m">The m channel.</param>
        /// <param name="y">The y channel.</param>
        /// <param name="k">The k channel.</param>
        /// <returns>The RGB color.</returns>
        private Color32 CMYKToRGB(byte c, byte m, byte y, byte k)
        {
            double num1 = Math.Pow(2.0, 8.0);
            double num6 = 1.0 - (c / num1);
            double num7 = 1.0 - (m / num1);
            double num8 = 1.0 - (y / num1);
            double num9 = 1.0 - (k / num1);

            byte red = (byte)((1.0 - ((num6 * (1.0 - num9)) + num9)) * byte.MaxValue);
            byte green = (byte)((1.0 - ((num7 * (1.0 - num9)) + num9)) * byte.MaxValue);
            byte blue = (byte)((1.0 - ((num8 * (1.0 - num9)) + num9)) * byte.MaxValue);

            if (red > byte.MaxValue)
            {
                red = byte.MaxValue;
            }

            if (green > byte.MaxValue)
            {
                green = byte.MaxValue;
            }

            if (blue > byte.MaxValue)
            {
                blue = byte.MaxValue;
            }

            return new Color32(red, green, blue, 1);
        }
    }
}
