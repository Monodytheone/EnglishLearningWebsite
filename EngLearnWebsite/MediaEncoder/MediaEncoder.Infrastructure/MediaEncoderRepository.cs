using MediaEncoder.Domain;
using MediaEncoder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaEncoder.Infrastructure;

public class MediaEncoderRepository : IMediaEncoderRepository
{
    private readonly MEDbContext _dbContext;

    public MediaEncoderRepository(MEDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<EncodingItem>> FindByStatusAsync(EncodingStatus status)
    {
        return _dbContext.EncodingItems.Where(item => item.Status == status).ToListAsync();
    }

    public Task<EncodingItem?> FindCompletedItemAsync(string fileSHA256Hash, long fileSizeInByte)
    {
        return _dbContext.EncodingItems.SingleOrDefaultAsync(item => item.FileSHA256Hash == fileSHA256Hash && item.FileSizeInByte == fileSizeInByte && item.Status == EncodingStatus.Completed);
    }
}
