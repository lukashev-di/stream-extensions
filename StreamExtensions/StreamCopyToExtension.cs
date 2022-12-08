namespace StreamExtensions
{
    public static partial class StreamCopyExtension
    {
        public static void CopyTo(this Stream stream, Stream destination, IProgress<int>? progress = default, CancellationToken cancellationToken = default)
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
                var dataToCopy = Read(stream, (int)partSize);
                if (dataToCopy == null || cancellationToken.IsCancellationRequested) break;
                destination.Write(dataToCopy, 0, dataToCopy.Length);

                var pr = (int)(100 * partNumber / partCount);
                if (pr <= percentage) continue;
                percentage = pr;
                progress?.Report(percentage);
            }
        }
        
        private static byte[]? Read(Stream data, int currentPartSize)
        {
            var result = new byte[currentPartSize];
            var totalRead = 0;
            while (totalRead < currentPartSize)
            {
                var curData = new byte[currentPartSize - totalRead];
                var curRead = data.Read(curData, 0, currentPartSize - totalRead);
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