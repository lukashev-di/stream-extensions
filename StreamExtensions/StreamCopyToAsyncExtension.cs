namespace StreamExtensions
{
    public static partial class StreamCopyExtension
    {
        public static async Task CopyToAsync(this Stream stream, Stream destination, IProgress<int>? progress = default, CancellationToken cancellationToken = default)
        {
            var percentage = 0;
            progress?.Report(percentage);
            
            var parts = Parts.CalculateMultiPartSize(stream.Length);
            var partSize = parts.PartSize;
            var partCount = parts.PartCount;
            var lastPartSize = parts.LastPartSize;
            
            for (var partNumber = 1; partNumber <= partCount; partNumber++)
            {
                if (partNumber == partCount) partSize = lastPartSize;
                var dataToCopy = await ReadAsync(stream, (int)partSize, cancellationToken).ConfigureAwait(false);
                if (dataToCopy == null || cancellationToken.IsCancellationRequested) break;
                await destination.WriteAsync(dataToCopy, 0, dataToCopy.Length, cancellationToken);

                var pr = (int)(100 * partNumber / partCount);
                if (pr <= percentage) continue;
                percentage = pr;
                progress?.Report(percentage);
            }
        }
        
        private static async Task<byte[]?> ReadAsync(Stream data, int currentPartSize, CancellationToken cancellationToken)
        {
            var result = new byte[currentPartSize];
            var totalRead = 0;
            while (totalRead < currentPartSize)
            {
                var curData = new byte[currentPartSize - totalRead];
                var curRead = await data.ReadAsync(curData, 0, currentPartSize - totalRead, cancellationToken).ConfigureAwait(false);
                if (curRead == 0) break;
                Buffer.BlockCopy(curData, 0, result, totalRead, curRead);
                totalRead += curRead;
            }

            if (totalRead == 0) return null;
            if (totalRead == currentPartSize) return result;

            var truncatedResult = new byte[totalRead];
            Buffer.BlockCopy(result, 0, truncatedResult, 0, totalRead);
            return truncatedResult;
        }
    }
}