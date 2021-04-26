using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GCore.Antlr.Grammers
{
    public class Helper
    {
        public static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 4096;

            using (var sourceStream = 
                new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))

            using (var destinationStream = 
                new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, fileOptions))

                await sourceStream.CopyToAsync(destinationStream, bufferSize)
                                        .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}