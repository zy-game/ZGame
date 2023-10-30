using System;

namespace ZGame
{
    public enum ErrorCode : int
    {
        OK = 200,
        NOT_FIND = 404,
        UPDATE_RESOURCE_FAIL = 405,
        LOAD_RESOURCE_OBJECT_FAIL = 406,
        DOWNLOAD_FAIL = 407,
        LOAD_ASSET_BUNDLE_FAIL = 408,
        PARAMETER_IS_EMPTY = 409,
        READ_FILE_FAIL = 410,
    }
}