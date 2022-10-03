﻿
namespace FileService.Domain;

public enum StorageType
{
    /// <summary>
    /// 供公众访问的存储设备
    /// </summary>
    Public,

    /// <summary>
    /// 内网备份使用的存储设备
    /// </summary>
    BackUp,
}
