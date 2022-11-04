using FFmpeg.NET;
using MediaEncoder.Domain;
using MediaEncoder.Domain.Entities;

namespace MediaEncoder.Infrastructure
{
    public class ToM4AEncoder : IMediaEncoder
    {
        public bool Accept(MediaFormat destFormat)
        {
            return destFormat == MediaFormat.m4a;
        }

        public async Task EncodeAsync(FileInfo sourceFile, FileInfo destFile, MediaFormat destFormat, string[]? args, CancellationToken ct)
        {
            //可以用“FFmpeg.AutoGen”，因为他是bingding库，不用启动独立的进程，更靠谱。但是编程难度大，这里重点不是FFMPEG，所以先用命令行实现
            // 需要把ffmpeg.exe设置为"如果较新则赋值"，这样它会被拷贝到程序的运行目录底下
            var inputFile = new InputFile(sourceFile);
            var outputFile = new OutputFile(destFile);
            string baseDir = AppContext.BaseDirectory;  // 程序的运行根目录
            string ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");
            var ffmpeg = new Engine(ffmpegPath);  // 用了xFFMpeg.Net包，把命令行操作封装了一下，这样比较简单
            string? errorMsg = null;
            ffmpeg.Error += (s, e) =>
            {
                errorMsg = e.Exception.Message;
            };
            await ffmpeg.ConvertAsync(inputFile, outputFile, ct);
            if (errorMsg != null)
            {
                throw new Exception(errorMsg);
            }
        }
    }
}
