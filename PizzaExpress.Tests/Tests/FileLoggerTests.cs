using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApplication3.Services;

namespace PizzaExpress.Tests.Tests
{
    /// <summary>
    /// Tests for <see cref="FileLogger"/> and <see cref="NullLogger"/>.
    /// FileLogger tests write to an isolated temp directory.
    /// </summary>
    [TestClass]
    public class FileLoggerTests
    {
        private string _tempDir;

        [TestInitialize]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),
                "PizzaExpressLogTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        private FileLogger MakeLogger() => new FileLogger(_tempDir);

        private string ReadLog()
        {
            string path = Path.Combine(_tempDir, $"app_{DateTime.Today:yyyy-MM-dd}.log");
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        // ── FileLogger ────────────────────────────────────────────────────────

        [TestMethod]
        public void Info_WritesEntryToFile()
        {
            MakeLogger().Info("Test info message");
            Assert.IsTrue(ReadLog().Contains("Test info message"));
        }

        [TestMethod]
        public void Info_ContainsInfoLevel()
        {
            MakeLogger().Info("hello");
            Assert.IsTrue(ReadLog().Contains("[INFO"));
        }

        [TestMethod]
        public void Warn_ContainsWarnLevel()
        {
            MakeLogger().Warn("a warning");
            Assert.IsTrue(ReadLog().Contains("[WARN"));
        }

        [TestMethod]
        public void Error_ContainsErrorLevel()
        {
            MakeLogger().Error("an error");
            Assert.IsTrue(ReadLog().Contains("[ERROR"));
        }

        [TestMethod]
        public void Error_WithException_IncludesExceptionType()
        {
            MakeLogger().Error("fail", new InvalidOperationException("boom"));
            string log = ReadLog();
            Assert.IsTrue(log.Contains("InvalidOperationException"));
            Assert.IsTrue(log.Contains("boom"));
        }

        [TestMethod]
        public void MultipleEntries_AllAppended()
        {
            var logger = MakeLogger();
            logger.Info("first");
            logger.Warn("second");
            logger.Error("third");
            string log = ReadLog();
            Assert.IsTrue(log.Contains("first"));
            Assert.IsTrue(log.Contains("second"));
            Assert.IsTrue(log.Contains("third"));
        }

        [TestMethod]
        public void Log_CreatesDirectoryIfMissing()
        {
            string subDir = Path.Combine(_tempDir, "NewLogDir");
            var logger    = new FileLogger(subDir);
            logger.Info("creates dir");
            Assert.IsTrue(Directory.Exists(subDir));
        }

        [TestMethod]
        public void Log_NeverThrows_WhenPathIsReadOnly()
        {
            // If logger can't write, it must swallow the error silently
            var logger = new FileLogger(@"Z:\NonExistentDrive\Logs");
            logger.Info("should not throw");  // must not throw
        }

        [TestMethod]
        public void Log_ContainsTimestamp()
        {
            MakeLogger().Info("ts check");
            // Timestamp format HH:mm:ss.fff — just check digits and colons present
            string log = ReadLog();
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(log, @"\d{2}:\d{2}:\d{2}"));
        }

        // ── NullLogger ────────────────────────────────────────────────────────

        [TestMethod]
        public void NullLogger_Info_DoesNotThrow()
            => NullLogger.Instance.Info("anything");

        [TestMethod]
        public void NullLogger_Warn_DoesNotThrow()
            => NullLogger.Instance.Warn("anything");

        [TestMethod]
        public void NullLogger_Error_DoesNotThrow()
            => NullLogger.Instance.Error("anything", new Exception("test"));

        [TestMethod]
        public void NullLogger_Error_NullException_DoesNotThrow()
            => NullLogger.Instance.Error("no ex", null);

        [TestMethod]
        public void NullLogger_ImplementsILogger()
        {
            ILogger logger = NullLogger.Instance;
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public void NullLogger_SharedInstance_IsSameObject()
            => Assert.AreSame(NullLogger.Instance, NullLogger.Instance);
    }
}
