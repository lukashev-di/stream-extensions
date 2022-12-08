namespace StreamExtensions
{
    internal class Parts
    {
        public long PartSize { get; private set; }
        public long PartCount { get; private set; }
        public long LastPartSize { get; private set; }

        public static Parts CalculateMultiPartSize(long size, long partSize = 8 * 1024L * 1024L)
        {
            var partCount = size / partSize;
            var lastPartSize = size - (partCount - 1) * partSize;
            var parts = new Parts
            {
                PartSize = partSize,
                PartCount = partCount,
                LastPartSize = lastPartSize
            };
            return parts;
        }
    }
}