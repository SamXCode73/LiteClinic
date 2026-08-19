using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public class DatabaseBackupService
    {
        private readonly string _sourcePath = "main.db";
        private readonly string _backupFolder = "backup";

        public void BackupAndZipDatabase()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string tempCopyPath = Path.Combine(_backupFolder, $"main_{timestamp}.db");
            string zipPath = Path.Combine(_backupFolder, $"main_{timestamp}.zip");

            Directory.CreateDirectory(_backupFolder);

            // Step 1: Copy the database file
            File.Copy(_sourcePath, tempCopyPath, overwrite: true);

            // Step 2: Create ZIP archive
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(tempCopyPath, $"main_{timestamp}.db");
            }

            // Step 3: Delete temp copy
            File.Delete(tempCopyPath);
        }
    }
}
