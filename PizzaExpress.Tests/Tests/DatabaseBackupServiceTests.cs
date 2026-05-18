using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Infrastructure;

namespace PizzaExpress.Tests.Tests
{
    [TestClass]
    public class DatabaseBackupServiceTests
    {
        private string _tempDir;

        [TestInitialize]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),
                "PizzaExpressBackupTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // ── RunAutoBackup ─────────────────────────────────────────────────────

        [TestMethod]
        public void RunAutoBackup_WhenDbAbsent_DoesNotThrow()
        {
            // No database file — should be a silent no-op
            DatabaseBackupService.RunAutoBackup(_tempDir);
            string backupDir = Path.Combine(_tempDir, "Backups");
            Assert.IsFalse(Directory.Exists(backupDir));
        }

        [TestMethod]
        public void RunAutoBackup_WhenDbPresent_CreatesDateStampedBackup()
        {
            CreateFakeDb(_tempDir);
            DatabaseBackupService.RunAutoBackup(_tempDir);

            string[] backups = DatabaseBackupService.GetAutoBackups(_tempDir);
            Assert.AreEqual(1, backups.Length);
            StringAssert.Contains(Path.GetFileName(backups[0]), "orders_auto_");
            StringAssert.Contains(Path.GetFileName(backups[0]), DateTime.Today.ToString("yyyyMMdd"));
        }

        [TestMethod]
        public void RunAutoBackup_CalledTwiceOnSameDay_DoesNotDuplicate()
        {
            CreateFakeDb(_tempDir);
            DatabaseBackupService.RunAutoBackup(_tempDir);
            DatabaseBackupService.RunAutoBackup(_tempDir);

            string[] backups = DatabaseBackupService.GetAutoBackups(_tempDir);
            Assert.AreEqual(1, backups.Length);
        }

        [TestMethod]
        public void RunAutoBackup_PrunesOldBackupsKeepingLast7()
        {
            CreateFakeDb(_tempDir);
            string backupDir = Path.Combine(_tempDir, "Backups");
            Directory.CreateDirectory(backupDir);

            // Pre-seed 8 old dated files
            for (int i = 1; i <= 8; i++)
            {
                string date = DateTime.Today.AddDays(-i).ToString("yyyyMMdd");
                File.WriteAllText(Path.Combine(backupDir, $"orders_auto_{date}.db"), "fake");
            }

            DatabaseBackupService.RunAutoBackup(_tempDir);

            // Today's backup + 7 older = 8 max; the oldest should have been pruned
            string[] backups = DatabaseBackupService.GetAutoBackups(_tempDir);
            Assert.AreEqual(7, backups.Length);
        }

        // ── BackupTo ──────────────────────────────────────────────────────────

        [TestMethod]
        public void BackupTo_WhenDbPresent_CopiesFileToDestination()
        {
            CreateFakeDb(_tempDir, content: "original content");
            string dest = Path.Combine(_tempDir, "my_backup.db");

            DatabaseBackupService.BackupTo(_tempDir, dest);

            Assert.IsTrue(File.Exists(dest));
            Assert.AreEqual("original content", File.ReadAllText(dest));
        }

        [TestMethod]
        public void BackupTo_WhenDbAbsent_ThrowsFileNotFoundException()
        {
            string dest = Path.Combine(_tempDir, "backup.db");
            Assert.ThrowsException<FileNotFoundException>(() =>
                DatabaseBackupService.BackupTo(_tempDir, dest));
        }

        [TestMethod]
        public void BackupTo_EmptyDestination_ThrowsArgumentException()
        {
            CreateFakeDb(_tempDir);
            Assert.ThrowsException<ArgumentException>(() =>
                DatabaseBackupService.BackupTo(_tempDir, "  "));
        }

        // ── RestoreFrom ───────────────────────────────────────────────────────

        [TestMethod]
        public void RestoreFrom_ReplacesLiveDbAndCreatesSafetyCopy()
        {
            CreateFakeDb(_tempDir, content: "live data");
            string sourceBackup = Path.Combine(_tempDir, "backup.db");
            File.WriteAllText(sourceBackup, "backup data");

            string safetyPath = DatabaseBackupService.RestoreFrom(_tempDir, sourceBackup);

            Assert.IsTrue(File.Exists(safetyPath));
            Assert.AreEqual("live data", File.ReadAllText(safetyPath));
            Assert.AreEqual("backup data", File.ReadAllText(Path.Combine(_tempDir, "orders.db")));
        }

        [TestMethod]
        public void RestoreFrom_WhenSourceAbsent_ThrowsFileNotFoundException()
        {
            CreateFakeDb(_tempDir);
            Assert.ThrowsException<FileNotFoundException>(() =>
                DatabaseBackupService.RestoreFrom(_tempDir, Path.Combine(_tempDir, "missing.db")));
        }

        [TestMethod]
        public void RestoreFrom_WhenNoLiveDb_StillCopiesBackupInPlace()
        {
            string sourceBackup = Path.Combine(_tempDir, "backup.db");
            File.WriteAllText(sourceBackup, "restore content");

            // No live DB — no safety copy created, but restore should still succeed
            DatabaseBackupService.RestoreFrom(_tempDir, sourceBackup);

            Assert.AreEqual("restore content", File.ReadAllText(Path.Combine(_tempDir, "orders.db")));
        }

        // ── GetDatabaseSizeKb ─────────────────────────────────────────────────

        [TestMethod]
        public void GetDatabaseSizeKb_WhenDbAbsent_ReturnsZero()
        {
            Assert.AreEqual(0L, DatabaseBackupService.GetDatabaseSizeKb(_tempDir));
        }

        [TestMethod]
        public void GetDatabaseSizeKb_WhenDbPresent_ReturnsNonNegative()
        {
            CreateFakeDb(_tempDir, content: new string('x', 2048));
            long kb = DatabaseBackupService.GetDatabaseSizeKb(_tempDir);
            Assert.IsTrue(kb >= 0);
        }

        // ── GetAutoBackups ────────────────────────────────────────────────────

        [TestMethod]
        public void GetAutoBackups_WhenNoBackupDir_ReturnsEmpty()
        {
            string[] result = DatabaseBackupService.GetAutoBackups(_tempDir);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void GetAutoBackups_ReturnsNewestFirst()
        {
            string backupDir = Path.Combine(_tempDir, "Backups");
            Directory.CreateDirectory(backupDir);
            File.WriteAllText(Path.Combine(backupDir, "orders_auto_20260101.db"), "a");
            File.WriteAllText(Path.Combine(backupDir, "orders_auto_20260103.db"), "c");
            File.WriteAllText(Path.Combine(backupDir, "orders_auto_20260102.db"), "b");

            string[] result = DatabaseBackupService.GetAutoBackups(_tempDir);

            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(string.Compare(
                Path.GetFileName(result[0]),
                Path.GetFileName(result[1]),
                StringComparison.Ordinal) > 0,
                "Results should be newest-first");
        }

        // ── PruneAutoBackups catch branch ─────────────────────────────────────

        [TestMethod]
        public void RunAutoBackup_WhenOldestFileLocked_SwallowsDeleteException()
        {
            CreateFakeDb(_tempDir);
            string backupDir = Path.Combine(_tempDir, "Backups");
            Directory.CreateDirectory(backupDir);

            // Pre-seed 7 old files; RunAutoBackup adds today's making 8 total.
            // AutoBackupKeep = 7, so index 7 (the oldest) must be deleted.
            for (int i = 1; i <= 7; i++)
            {
                string date = DateTime.Today.AddDays(-i).ToString("yyyyMMdd");
                File.WriteAllText(Path.Combine(backupDir, $"orders_auto_{date}.db"), "fake");
            }

            // Lock the oldest file so File.Delete throws — covers the catch block.
            string oldest = Path.Combine(backupDir,
                $"orders_auto_{DateTime.Today.AddDays(-7):yyyyMMdd}.db");
            using (File.Open(oldest, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                DatabaseBackupService.RunAutoBackup(_tempDir);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void CreateFakeDb(string dataDir, string content = "fake db content")
        {
            File.WriteAllText(Path.Combine(dataDir, "orders.db"), content);
        }
    }
}
