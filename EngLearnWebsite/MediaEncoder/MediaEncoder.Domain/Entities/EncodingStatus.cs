using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoder.Domain.Entities
{
    public enum EncodingStatus
    {
        /// <summary>
        /// 转码任务刚刚创建
        /// </summary>
        Ready,

        /// <summary>
        /// 转码已开始
        /// </summary>
        Started,

        /// <summary>
        /// 转码已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 转码失败
        /// </summary>
        Failed,
    }
}
