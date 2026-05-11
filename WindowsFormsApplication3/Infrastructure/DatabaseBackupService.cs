using System;
using System.IO;
using System.Linq;

namespace WindowsFormsApplication3.Infrastructure
{
    /// <summary>
    /// Manages SQLite database backups for Pizza Express NZ.
    ///
    /// Strategy:
    ///   - On each application startup, a rolling auto-backup is created in
    ///     %APPDATA%\PizzaExpress\Backups\  named  orders_auto_YYYYMMDD.db
    ///     Only the 7 most recent auto-backups are kept; older ones are pruned.
    ///   - Manual backups are saved wherever the user chooses via SaveFileDialog.
    ///   - Restore replaces the live orders.db with a chosen backup file.
    ///     The live file is preserved as orders_prerestore_TIMESTAMP.db before
    ///     overwriting so the restore itself is reversible.
    /// </summary>
    public static class DatabaseBackupService
    {
        private const string DbFileName     = "orders.db";
        private const string BackupFolder   = "Backups";
        private const int    AutoBackupKeep = 7;

        // ── Auto-backup (called at startup) ──────────────────────────────────

        /// <summary>
        /// Creates a dated auto-backup of the live database and prunes old ones.
        /// Safe to call even if the database does not yet exist.
        /// </summary>
        public static void RunAutoBackup(string dataDirectory)
        {
            string dbPath = Path.Combine(dataDirectory, DbFileName);
            if (!File.Exists(dbPath)) return;

            string backupDir = EnsureBackupDir(dataDirectory);
            string dest      = Path.Combine(backupDir,
                $"orders_auto_{DateTime.Today:yyyyMMdd}.db");

            // Only copy once per day — don't overwrite an existing today-backup
            if (!File.Exists(dest))
                File.Copy(dbPath, dest, overwrite: false);

            PruneAutoBackups(backupDir);
        }

        // ── Manual backup ─────────────────────────────────────────────────────

        /// <summary>
        /// Copies the live database to <paramref name="destinationPath"/>.
        /// Returns the destination path on success, or throws on failure.
        /// </summary>
        public static string BackupTo(string dataDirectory, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path must not be empty.");

            string dbPath = Path.Combine(dataDirectory, DbFileName);
            if (!File.Exists(dbPath))
                throw new FileNotFoundException(
                    "No database found to back up. Place at least one order first.", dbPath);

            File.Copy(dbPath, destinationPath, overwrite: true);
            return destinationPath;
        }

        // ── Restore ───────────────────────────────────────────────────────────

        /// <summary>
        /// Restores the live database from <paramref name="sourcePath"/>.
        /// Saves a safety copy of the current live database before overwriting.
        /// Returns the path of the safety copy.
        /// </summary>
        public static string RestoreFrom(string dataDirectory, string sourcePath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Backup file not found.", sourcePath);

            string dbPath     = Path.Combine(dataDirectory, DbFileName);
            string backupDir  = EnsureBackupDir(dataDirectory);
            string safetyPath = Path.Combine(backupDir,
                $"orders_prerestore_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            // Preserve current live DB before overwriting
            if (File.Exists(dbPath))
                File.Copy(dbPath, safetyPath, overwrite: false);

            File.Copy(sourcePath, dbPath, overwrite: true);
            return safetyPath;
        }

        // ── Info ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the size of the live database in kilobytes, or 0 if absent.
        /// </summary>
        public static long GetDatabaseSizeKb(string dataDirectory)
        {
            string dbPath = Path.Combine(dataDirectory, DbFileName);
            if (!File.Exists(dbPath)) return 0;
            return new FileInfo(dbPath).Length / 1024;
        }

        /// <summary>
        /// Returns the paths of all auto-backups, newest first.
        /// </summary>
        public static string[] GetAutoBackups(string dataDirectory)
        {
            string backupDir = Path.Combine(dataDirectory, BackupFolder);
            if (!Directory.Exists(backupDir)) return Array.Empty<string>();

            return Directory
                .GetFiles(backupDir, "orders_auto_*.db")
                .OrderByDescending(f => f)
                .ToArray();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static string EnsureBackupDir(string dataDirectory)
        {
            string dir = Path.Combine(dataDirectory, BackupFolder);
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void PruneAutoBackups(string backupDir)
        {
            string[] files = Directory
                .GetFiles(backupDir, "orders_auto_*.db")
                .OrderByDescending(f => f)
                .ToArray();

            for (int i = AutoBackupKeep; i < files.Length; i++)
            {
                try { File.Delete(files[i]); }
                catch { /* best-effort — never crash startup */ }
            }
        }
    }
}
