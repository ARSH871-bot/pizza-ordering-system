using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PizzaExpress.Tests.Tests
{
    internal static class WinFormsTestHelper
    {
        public static void RunInSta(Action action, int timeoutMs = 90000)
        {
            Exception failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    EnsureWinFormsAssemblyRedirects();
                    action();
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!thread.Join(timeoutMs))
                throw new AssertFailedException("The STA test timed out.");

            if (failure != null)
            {
                Console.WriteLine(failure);
                ExceptionDispatchInfo.Capture(failure).Throw();
            }
        }

        public static void PumpEvents(int extraMs = 0)
        {
            for (int i = 0; i < 3; i++)
            {
                Application.DoEvents();
                Thread.Sleep(50);
            }
            if (extraMs > 0)
                Thread.Sleep(extraMs);
            Application.DoEvents();
        }

        public static T FindByName<T>(Control root, string name) where T : Control
        {
            Control[] matches = root.Controls.Find(name, true);
            Assert.AreEqual(1, matches.Length, $"Expected one control named '{name}'.");
            Assert.IsInstanceOfType(matches[0], typeof(T));
            return (T)matches[0];
        }

        public static T FindByTextPrefix<T>(Control root, string textPrefix) where T : Control
        {
            foreach (T control in EnumerateControls<T>(root))
            {
                if ((control.Text ?? string.Empty).StartsWith(textPrefix, StringComparison.OrdinalIgnoreCase))
                    return control;
            }

            Assert.Fail($"Could not find a {typeof(T).Name} starting with '{textPrefix}'.");
            return null;
        }

        public static IEnumerable<T> EnumerateControls<T>(Control root) where T : Control
        {
            foreach (Control child in root.Controls)
            {
                if (child is T typed)
                    yield return typed;

                foreach (T descendant in EnumerateControls<T>(child))
                    yield return descendant;
            }
        }

        public static T GetPrivateField<T>(object target, string fieldName) where T : class
        {
            Assert.IsNotNull(target, "Target object must not be null.");

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");

            object value = field.GetValue(target);
            Assert.IsInstanceOfType(value, typeof(T), $"Field '{fieldName}' was not a {typeof(T).Name}.");
            return (T)value;
        }

        private static void EnsureWinFormsAssemblyRedirects()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveFromTestBin;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromTestBin;
        }

        private static Assembly ResolveFromTestBin(object sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name);
            if (string.Equals(requested.Name, "System.Runtime.CompilerServices.Unsafe", StringComparison.OrdinalIgnoreCase))
            {
                string unsafePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".nuget",
                    "packages",
                    "system.runtime.compilerservices.unsafe",
                    "4.5.3",
                    "lib",
                    "net461",
                    "System.Runtime.CompilerServices.Unsafe.dll");
                return File.Exists(unsafePath) ? Assembly.LoadFrom(unsafePath) : null;
            }

            if (requested.Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) == false)
                return null;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidate = Path.Combine(baseDir, requested.Name + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        }

        internal sealed class DialogAutoCloser : IDisposable
        {
            private readonly string[] _titleFragments;
            private readonly Thread _thread;
            private volatile bool _disposed;

            public DialogAutoCloser(params string[] titleFragments)
            {
                _titleFragments = titleFragments ?? Array.Empty<string>();
                _thread = new Thread(WatchLoop) { IsBackground = true };
                _thread.Start();
            }

            public void Dispose()
            {
                _disposed = true;
                _thread.Join(1000);
            }

            private void WatchLoop()
            {
                while (!_disposed)
                {
                    EnumWindows((hWnd, _) =>
                    {
                        string title = GetWindowTitle(hWnd);
                        if (string.IsNullOrWhiteSpace(title))
                            return true;

                        foreach (string fragment in _titleFragments)
                        {
                            if (!string.IsNullOrWhiteSpace(fragment) &&
                                title.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Try SendMessage first (synchronous) then PostMessage as fallback
                                SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                break;
                            }
                        }

                        return true;
                    }, IntPtr.Zero);

                    Thread.Sleep(50);
                }
            }

            private static string GetWindowTitle(IntPtr hWnd)
            {
                int length = GetWindowTextLength(hWnd);
                if (length <= 0)
                    return string.Empty;

                var builder = new StringBuilder(length + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            private const uint WM_CLOSE = 0x0010;

            private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
