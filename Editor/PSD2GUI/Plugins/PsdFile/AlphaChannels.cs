#region 脚本信息
/**************************************************************************************
* Copyright (c) 2015  All Rights Reserved.
* FileName	：AlphaChannels.cs
* Author	：
* CreateTime：2022/03/31 19:50:38
* version	：V1.0.0
* Description：
*=======================================================================================
* ModifyInfo:
* Modifier:
* ModifyTime:
* Version:
* Description:
*****************************************************************************************/
#endregion
namespace PhotoshopFile
{
    /// <summary>
    /// The names of the alpha channels
    /// </summary>
    public class AlphaChannels : ImageResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaChannels" /> class.
        /// </summary>
        /// <param name="imgRes">The image resource.</param>
        public AlphaChannels(ImageResource imgRes)
            : base(imgRes)
        {
            BinaryReverseReader dataReader = imgRes.DataReader;
            while (dataReader.BaseStream.Length - dataReader.BaseStream.Position > 0L)
            {
                // read the length of the string
                byte length = dataReader.ReadByte();

                // read the string
                dataReader.ReadChars(length);
            }

            dataReader.Close();
        }
    }
}
