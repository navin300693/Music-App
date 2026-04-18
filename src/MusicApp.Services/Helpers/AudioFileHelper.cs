namespace MusicApp.Services.Helpers;

public static class AudioFileHelper
{
    public static TimeSpan ExtractDuration(string fileName, Stream stream)
    {
        using var tagFile = TagLib.File.Create(new StreamFileAbstraction(fileName, stream));
        return tagFile.Properties.Duration;
    }

    // TagLibSharp requires an IFileAbstraction to read from a stream instead of a file path
    private sealed class StreamFileAbstraction(string name, Stream stream) : TagLib.File.IFileAbstraction
    {
        public string Name => name;
        public Stream ReadStream => stream;
        public Stream WriteStream => stream;
        public void CloseStream(Stream s) { }
    }
}

