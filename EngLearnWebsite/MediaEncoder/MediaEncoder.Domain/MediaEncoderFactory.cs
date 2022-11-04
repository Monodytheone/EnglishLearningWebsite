using MediaEncoder.Domain.Entities;

namespace MediaEncoder.Domain;

public class MediaEncoderFactory
{
    private readonly IEnumerable<IMediaEncoder> _encoders;

    public MediaEncoderFactory(IEnumerable<IMediaEncoder> encoders)  // 将所有IMediaEncoder全部注入进来（不能注入东西给静态类哦）
    {
        _encoders = encoders;
    }

    public IMediaEncoder? Create(MediaFormat destFormat)
    {
        foreach (IMediaEncoder encoder in _encoders)
        {
            if (encoder.Accept(destFormat))
            {
                return encoder;
            }
        }
        return null;
    }
}
