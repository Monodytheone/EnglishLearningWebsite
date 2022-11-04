using MediaEncoder.Domain.Entities;

namespace MediaEncoder.Domain;

public interface IMediaEncoder
{
    /// <summary>
    /// 本转码器是否能将文件转码为destFormat格式
    /// </summary>
    /// <param name="destFormat">目标格式</param>
    /// <returns></returns>
    bool Accept(MediaFormat destFormat);

    /// <summary>
    /// 进行转码
    /// </summary>
    /// <returns></returns>
    Task EncodeAsync(FileInfo sourceFile, FileInfo destFile, MediaFormat destFormat, string[]? args,
        CancellationToken ct);
}
