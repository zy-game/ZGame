#region 脚本信息
/**************************************************************************************
* Copyright (c) 2015  All Rights Reserved.
* FileName	：BlendingRanges.cs
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
    /// The blending ranges for a layer
    /// </summary>
    public class BlendingRanges
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlendingRanges"/> class.
        /// </summary>
        /// <param name="reader">The reader containing the PSD file data</param>
        public BlendingRanges(BinaryReverseReader reader)
        {
            // read the data length
            int count = reader.ReadInt32();
            if (count <= 0)
            {
                return;
            }

            // read the data
            reader.ReadBytes(count);
        }
    }
}
