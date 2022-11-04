using MediaEncoder.Domain.Entities;
using System.Text;

namespace MediaEncoder.Domain
{
    public interface IMediaEncoderRepository
    {
        /// <summary>
        /// 根据散列值和文件大小，获取已完成转码的EncodingItem
        /// </summary>
        /// <param name="fileSHA256Hash"></param>
        /// <param name="fileSizeInByte"></param>
        /// <returns></returns>
        Task<EncodingItem?> FindCompletedItemAsync(string fileSHA256Hash, long fileSizeInByte);

        /// <summary>
        /// 获取处于某种转码状态的全部EncodingItem
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<List<EncodingItem>> FindByStatusAsync(EncodingStatus status);
    }
}
