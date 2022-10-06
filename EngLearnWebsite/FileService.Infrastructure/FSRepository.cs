using FileService.Domain;
using FileService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure
{
    public class FSRepository : IFSRepository
    {
        private readonly FSDbContext _fsDbContext;

        public FSRepository(FSDbContext fsDbContext)
        {
            _fsDbContext = fsDbContext;
        }

        public Task<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash)
        {
            return _fsDbContext.UploadedItems.FirstOrDefaultAsync(x => x.FileSizeInBytes == fileSize && x.FileSHA256Hash == sha256Hash);  // 两个文件大小相同且哈希值相同，我们就认为它们是相同的文件
        }
    }
}
