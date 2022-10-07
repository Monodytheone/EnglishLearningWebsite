using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime CreationTime { get; init; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeletionTime { get; private set; }

        /// <summary>
        /// 软删除标志
        /// </summary>
        public bool IsDeleted { get; private set; }

        public User(string userName) : base(userName)
        {
            Id = Guid.NewGuid();
            CreationTime = DateTime.Now;
            IsDeleted = false;
        }

        /// <summary>
        /// 软删除
        /// </summary>
        public void SoftDelete()
        {
            IsDeleted = true;
            DeletionTime = DateTime.Now;
        }

    }
}
