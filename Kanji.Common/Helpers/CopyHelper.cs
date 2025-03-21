﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Common.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Copies all content from directory "source" to directory "destination".
        /// </summary>
        /// <param name="source">Source directory path.</param>
        /// <param name="destination">Destination directory path.</param>
        public static void CopyAllContent(string source, string destination)
        {
            // Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
            {
                string newPath = dirPath.Replace(source, destination);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            // Copy all the files
            foreach (string filePath in Directory.GetFiles(source, "*",
                SearchOption.AllDirectories))
            {
                string newPath = filePath.Replace(source, destination);

                if (!File.Exists(newPath))
                {
                    File.Copy(filePath, newPath);
                }
            }
        }

        /// <summary>
        /// Copies the source file to the destination path if there is not already an existing
        /// and identical file at the destination path.
        /// </summary>
        /// <param name="sourceFilePath">Source file path.</param>
        /// <param name="destinationFilePath">Destination file path.</param>
        public static void CopyIfDifferent(Func<Stream> sourceFile, string destinationFilePath)
        {
            if (!File.Exists(destinationFilePath) || !FileEquals(sourceFile(), destinationFilePath))
            {
                using var inFile = sourceFile();
                using var outFile = new FileStream(destinationFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                inFile.CopyTo(outFile);
            }
        }

        // Thank you http://stackoverflow.com/questions/7931304/comparing-two-files-in-c-sharp
        /// <summary>
        /// Tests if the two specified files are identical.
        /// </summary>
        /// <param name="pathA">First file path.</param>
        /// <param name="pathB">Second file path.</param>
        /// <returns></returns>
        public static bool FileEquals(Stream fileA, string pathB)
        {
            int file1byte;
            int file2byte;

            // Open the two files.
            using var fs1 = fileA;
            using var fs2 = new FileStream(pathB, FileMode.Open, FileAccess.Read);
            try
            {
                // Check the file sizes. If they are not the same, the files 
                // are not the same.
                if (fs1.Length != fs2.Length)
                {
                    // Return false to indicate files are different
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                return fs2.Length > 0;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return (file1byte - file2byte) == 0;
        }
    }
}
