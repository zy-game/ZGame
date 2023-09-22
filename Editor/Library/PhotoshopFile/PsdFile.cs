using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

namespace PhotoshopFile
{
    public class PsdFile
    {
        private class DecompressChannelContext
        {
            private Channel ch;

            public DecompressChannelContext(Channel ch)
            {
                this.ch = ch;
            }

            public void DecompressChannel(object context)
            {
                ch.DecodeImageData();
            }
        }
        public string name { get; set; }

        private short channelCount;

        private int bitDepth;


        public byte[] ColorModeData = new byte[0];

        private byte[] GlobalLayerMaskData = new byte[0];

        public Layer BaseLayer { get; set; }

        public ImageCompression ImageCompression { get; set; }

        public short Version { get; private set; }

        public short ChannelCount
        {
            get
            {
                return channelCount;
            }
            set
            {
                if (value < 1 || value > 56)
                {
                    throw new ArgumentException("Number of channels must be from 1 to 56.");
                }
                channelCount = value;
            }
        }


        public int RowCount
        {
            get
            {
                return (int)BaseLayer.Rect.height;
            }
            set
            {
                if (value < 0 || value > 30000)
                {
                    throw new ArgumentException("Number of rows must be from 1 to 30000.");
                }
                BaseLayer.Rect = new Rect(0f, 0f, BaseLayer.Rect.width, value);
            }
        }

        public int ColumnCount
        {
            get
            {
                return (int)BaseLayer.Rect.width;
            }
            set
            {
                if (value < 0 || value > 30000)
                {
                    throw new ArgumentException("Number of columns must be from 1 to 30000.");
                }
                BaseLayer.Rect = new Rect(0f, 0f, value, BaseLayer.Rect.height);
            }
        }

        public int BitDepth
        {
            get
            {
                return bitDepth;
            }
            set
            {
                switch (value)
                {
                    case 1:
                    case 8:
                    case 16:
                    case 32:
                        bitDepth = value;
                        break;
                    default:
                        throw new NotImplementedException("Invalid bit depth.");
                }
            }
        }

        public PsdColorMode ColorMode { get; set; }

        public ImageResources ImageResources { get; set; }

        public ResolutionInfo Resolution
        {
            get
            {
                return (ResolutionInfo)ImageResources.Get(ResourceID.ResolutionInfo);
            }
            set
            {
                ImageResources.Set(value);
            }
        }

        public List<Layer> Layers { get; private set; }

        public List<LayerInfo> AdditionalInfo { get; private set; }

        public bool AbsoluteAlpha { get; set; }

        public PsdFile()
        {
            Version = 1;
            BaseLayer = new Layer(this);
            ImageResources = new ImageResources();
            Layers = new List<Layer>();
            AdditionalInfo = new List<LayerInfo>();
        }

        public PsdFile(string filename, Encoding encoding)
            : this()
        {
            name = Path.GetFileNameWithoutExtension(filename);
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                Load(stream, encoding);
            }
        }

        public PsdFile(Stream stream, Encoding encoding)
            : this()
        {
            Load(stream, encoding);
        }



        /// <summary>
        /// Transform Photoshop's layer tree to Paint.NET's flat layer list.
        /// Indicate where layer sections begin and end, and hide all layers within
        /// hidden layer sections.
        /// </summary>
        private static void ApplyLayerSections(List<Layer> layers)
        {
            // BUG: PsdPluginResources.GetString will always return English resource,
            // because Paint.NET does not set the CurrentUICulture when OnLoad is
            // called.  This situation should be resolved with Paint.NET 4.0, which
            // will provide an alternative mechanism to retrieve the UI language.

            // Track the depth of the topmost hidden section.  Any nested sections
            // will be hidden, whether or not they themselves have the flag set.
            int topHiddenSectionDepth = Int32.MaxValue;
            var layerSectionNames = new Stack<string>();

            // Layers are stored bottom-to-top, but layer sections are specified
            // top-to-bottom.
            foreach (var layer in Enumerable.Reverse(layers))
            {
                // Leo: Since we are importing, we don't care if the group is collapsed
                // Apply to all layers within the layer section, as well as the
                // closing layer.
                //if (layerSectionNames.Count > topHiddenSectionDepth)
                //    layer.Visible = false;

                var sectionInfo = (LayerSectionInfo)layer.AdditionalInfo
                    .SingleOrDefault(x => x is LayerSectionInfo);
                if (sectionInfo == null)
                    continue;

                switch (sectionInfo.SectionType)
                {
                    case LayerSectionType.OpenFolder:
                    case LayerSectionType.ClosedFolder:
                        // Start a new layer section
                        if ((!layer.Visible) && (topHiddenSectionDepth == Int32.MaxValue))
                            topHiddenSectionDepth = layerSectionNames.Count;
                        layerSectionNames.Push(layer.Name);
                        layer.IsGroup = true;
                        //layer.Name = String.Format(beginSectionWrapper, layer.Name);
                        break;

                    case LayerSectionType.SectionDivider:
                        // End the current layer section
                        //var layerSectionName = layerSectionNames.Pop ();
                        if (layerSectionNames.Count == topHiddenSectionDepth)
                            topHiddenSectionDepth = Int32.MaxValue;
                        layer.IsEndGroupMarker = true;
                        //layer.Name = String.Format(endSectionWrapper, layerSectionName);
                        break;
                }
            }
        }

        private void Load(Stream stream, Encoding encoding)
        {
            PsdBinaryReader reader = new PsdBinaryReader(stream, encoding);
            LoadHeader(reader);
            LoadColorModeData(reader);
            LoadImageResources(reader);
            LoadLayerAndMaskInfo(reader);
            LoadImage(reader);
            DecompressImages();
            VerifyLayerSections();
            ApplyLayerSections(Layers);
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].CreateTexture();
            }
        }

        public void Save(string fileName, Encoding encoding)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                Save(stream, encoding);
            }
        }

        public void Save(Stream stream, Encoding encoding)
        {
            if (BitDepth != 8)
            {
                throw new NotImplementedException("Only 8-bit color has been implemented for saving.");
            }
            PsdBinaryWriter psdBinaryWriter = new PsdBinaryWriter(stream, encoding);
            psdBinaryWriter.AutoFlush = true;
            PrepareSave();
            SaveHeader(psdBinaryWriter);
            SaveColorModeData(psdBinaryWriter);
            SaveImageResources(psdBinaryWriter);
            SaveLayerAndMaskInfo(psdBinaryWriter);
            SaveImage(psdBinaryWriter);
        }

        private void LoadHeader(PsdBinaryReader reader)
        {
            string text = reader.ReadAsciiChars(4);
            if (text != "8BPS")
            {
                throw new PsdInvalidException("The given stream is not a valid PSD file");
            }
            Version = reader.ReadInt16();
            if (Version != 1)
            {
                throw new PsdInvalidException("The PSD file has an unknown version");
            }
            reader.BaseStream.Position += 6L;
            ChannelCount = reader.ReadInt16();
            RowCount = reader.ReadInt32();
            ColumnCount = reader.ReadInt32();
            BitDepth = reader.ReadInt16();
            ColorMode = (PsdColorMode)reader.ReadInt16();
        }

        private void SaveHeader(PsdBinaryWriter writer)
        {
            string s = "8BPS";
            writer.WriteAsciiChars(s);
            writer.Write(Version);
            byte[] value = new byte[6];
            writer.Write(value);
            writer.Write(ChannelCount);
            writer.Write(RowCount);
            writer.Write(ColumnCount);
            writer.Write((short)BitDepth);
            writer.Write((short)ColorMode);
        }

        private void LoadColorModeData(PsdBinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num != 0)
            {
                ColorModeData = reader.ReadBytes((int)num);
            }
        }

        private void SaveColorModeData(PsdBinaryWriter writer)
        {
            writer.Write((uint)ColorModeData.Length);
            writer.Write(ColorModeData);
        }

        private void LoadImageResources(PsdBinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num != 0)
            {
                long position = reader.BaseStream.Position;
                long num2 = position + num;
                while (reader.BaseStream.Position < num2)
                {
                    ImageResource item = ImageResourceFactory.CreateImageResource(reader);
                    ImageResources.Add(item);
                }
                reader.BaseStream.Position = position + num;
            }
        }

        private void SaveImageResources(PsdBinaryWriter writer)
        {
            using (new PsdBlockLengthWriter(writer))
            {
                foreach (ImageResource imageResource in ImageResources)
                {
                    imageResource.Save(writer);
                }
            }
        }

        private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num == 0)
            {
                return;
            }
            long position = reader.BaseStream.Position;
            long num2 = position + num;
            LoadLayers(reader, true);
            LoadGlobalLayerMask(reader);
            while (reader.BaseStream.Position < num2)
            {
                LayerInfo layerInfo = LayerInfoFactory.Load(reader);
                AdditionalInfo.Add(layerInfo);
                if (!(layerInfo is RawLayerInfo))
                {
                    continue;
                }
                RawLayerInfo rawLayerInfo = (RawLayerInfo)layerInfo;
                switch (layerInfo.Key)
                {
                    case "Layr":
                    case "Lr16":
                    case "Lr32":
                        {
                            using (MemoryStream stream = new MemoryStream(rawLayerInfo.Data))
                            {
                                using (PsdBinaryReader reader2 = new PsdBinaryReader(stream, reader))
                                {
                                    LoadLayers(reader2, false);
                                }
                            }
                            break;
                        }
                    case "LMsk":
                        GlobalLayerMaskData = rawLayerInfo.Data;
                        break;
                }
            }
            reader.BaseStream.Position = position + num;
        }

        private void SaveLayerAndMaskInfo(PsdBinaryWriter writer)
        {
            using (new PsdBlockLengthWriter(writer))
            {
                long position = writer.BaseStream.Position;
                SaveLayers(writer);
                SaveGlobalLayerMask(writer);
                foreach (LayerInfo item in AdditionalInfo)
                {
                    item.Save(writer);
                }
                writer.WritePadding(position, 2);
            }
        }

        private void LoadLayers(PsdBinaryReader reader, bool hasHeader)
        {
            uint num = 0u;
            if (hasHeader)
            {
                num = reader.ReadUInt32();
                if (num == 0)
                {
                    return;
                }
            }
            long position = reader.BaseStream.Position;
            short num2 = reader.ReadInt16();
            if (num2 < 0)
            {
                AbsoluteAlpha = true;
                num2 = Math.Abs(num2);
            }
            if (num2 == 0)
            {
                return;
            }
            for (int i = 0; i < num2; i++)
            {
                Layer item = new Layer(reader, this);
                Layers.Add(item);
            }
            foreach (Layer layer in Layers)
            {
                foreach (Channel channel in layer.Channels)
                {
                    channel.LoadPixelData(reader);
                }
            }
            if (num != 0)
            {
                long num3 = position + num;
                long num4 = reader.BaseStream.Position - num3;
                if (reader.BaseStream.Position < num3)
                {
                    reader.BaseStream.Position = num3;
                }
            }
        }

        private void DecompressImages()
        {
            IEnumerable<Layer> enumerable = Layers.Concat(new List<Layer> { BaseLayer });
            foreach (Layer item in enumerable)
            {
                foreach (Channel channel in item.Channels)
                {
                    DecompressChannelContext @object = new DecompressChannelContext(channel);
                    WaitCallback callBack = @object.DecompressChannel;
                    ThreadPool.QueueUserWorkItem(callBack);
                }
            }
            foreach (Layer layer in Layers)
            {
                foreach (Channel channel2 in layer.Channels)
                {
                    if (channel2.ID == -2)
                    {
                        layer.Masks.LayerMask.ImageData = channel2.ImageData;
                    }
                    else if (channel2.ID == -3)
                    {
                        layer.Masks.UserMask.ImageData = channel2.ImageData;
                    }
                }
            }
        }

        public void PrepareSave()
        {
            List<Layer> list = Layers.Concat(new List<Layer> { BaseLayer }).ToList();
            foreach (Layer item in list)
            {
                item.PrepareSave();
            }
            SetVersionInfo();
            VerifyLayerSections();
        }

        internal void VerifyLayerSections()
        {
            int num = 0;
            foreach (Layer item in Enumerable.Reverse(Layers))
            {
                LayerInfo layerInfo = item.AdditionalInfo.SingleOrDefault((LayerInfo x) => x is LayerSectionInfo);
                if (layerInfo == null)
                {
                    continue;
                }
                LayerSectionInfo layerSectionInfo = (LayerSectionInfo)layerInfo;
                switch (layerSectionInfo.SectionType)
                {
                    case LayerSectionType.OpenFolder:
                    case LayerSectionType.ClosedFolder:
                        num++;
                        break;
                    case LayerSectionType.SectionDivider:
                        num--;
                        if (num < 0)
                        {
                            throw new PsdInvalidException("Layer section ended without matching start marker.");
                        }
                        break;
                    default:
                        throw new PsdInvalidException("Unrecognized layer section type.");
                }
            }
            if (num != 0)
            {
                throw new PsdInvalidException("Layer section not closed by end marker.");
            }
        }

        public void SetVersionInfo()
        {
            VersionInfo versionInfo = (VersionInfo)ImageResources.Get(ResourceID.VersionInfo);
            if (versionInfo == null)
            {
                versionInfo = new VersionInfo();
                ImageResources.Set(versionInfo);
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                Version version = executingAssembly.GetName().Version;
                string text = version.Major + "." + version.Minor + "." + version.Build;
                versionInfo.Version = 1u;
                versionInfo.HasRealMergedData = true;
                versionInfo.ReaderName = "Paint.NET PSD Plugin";
                versionInfo.WriterName = "Paint.NET PSD Plugin " + text;
                versionInfo.FileVersion = 1u;
            }
        }

        private void SaveLayers(PsdBinaryWriter writer)
        {
            using (new PsdBlockLengthWriter(writer))
            {
                short num = (short)Layers.Count;
                if (AbsoluteAlpha)
                {
                    num = (short)(-num);
                }
                if (num == 0)
                {
                    return;
                }
                long position = writer.BaseStream.Position;
                writer.Write(num);
                foreach (Layer layer in Layers)
                {
                    layer.Save(writer);
                }
                foreach (Layer layer2 in Layers)
                {
                    foreach (Channel channel in layer2.Channels)
                    {
                        channel.SavePixelData(writer);
                    }
                }
                writer.WritePadding(position, 4);
            }
        }

        private void LoadGlobalLayerMask(PsdBinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num != 0)
            {
                GlobalLayerMaskData = reader.ReadBytes((int)num);
            }
        }

        private void SaveGlobalLayerMask(PsdBinaryWriter writer)
        {
            writer.Write((uint)GlobalLayerMaskData.Length);
            writer.Write(GlobalLayerMaskData);
        }

        private void LoadImage(PsdBinaryReader reader)
        {
            ImageCompression = (ImageCompression)reader.ReadInt16();
            for (short num = 0; num < ChannelCount; num = (short)(num + 1))
            {
                Channel channel = new Channel(num, BaseLayer);
                channel.ImageCompression = ImageCompression;
                channel.Length = RowCount * Util.BytesPerRow(BaseLayer.Rect, BitDepth);
                if (ImageCompression == ImageCompression.Rle)
                {
                    channel.RleRowLengths = new RleRowLengths(reader, RowCount);
                    channel.Length = channel.RleRowLengths.Total;
                }
                BaseLayer.Channels.Add(channel);
            }
            foreach (Channel channel3 in BaseLayer.Channels)
            {
                channel3.ImageDataRaw = reader.ReadBytes(channel3.Length);
            }
            if (ColorMode != PsdColorMode.Multichannel && ChannelCount == ColorMode.MinChannelCount() + 1)
            {
                Channel channel2 = BaseLayer.Channels.Last();
                channel2.ID = -1;
            }
        }

        private void SaveImage(PsdBinaryWriter writer)
        {
            writer.Write((short)ImageCompression);
            if (ImageCompression == ImageCompression.Rle)
            {
                foreach (Channel channel in BaseLayer.Channels)
                {
                    channel.RleRowLengths.Write(writer);
                }
            }
            foreach (Channel channel2 in BaseLayer.Channels)
            {
                writer.Write(channel2.ImageDataRaw);
            }
        }
    }
}
