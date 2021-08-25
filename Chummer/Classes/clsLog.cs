/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Chummer
{
    [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
    public static class Log
    {
        private static StreamWriter _logWriter;
        private static readonly object s_LogWriterLock = new object();

        static Log()
        {
            Stopwatch sw = Stopwatch.StartNew();
            IsLoggerEnabled = GlobalOptions.UseLogging;
            sw.TaskEnd("log open");
        }

        private static bool _blnIsLoggerEnabled;

        public static bool IsLoggerEnabled
        {
            get => _blnIsLoggerEnabled;
            set
            {
                lock (s_LogWriterLock)
                {
                    if (_blnIsLoggerEnabled != value)
                    {
                        // Sets up logging information
                        if (value)
                        {
                            _logWriter = new StreamWriter(Path.Combine(Utils.GetStartupPath, "chummerlog.txt"));
                        }
                        // This will disabled logging and free any resources used by it
                        else if (_logWriter != null)
                        {
                            _logWriter.Flush();
                            _logWriter.Close();
                        }

                        _blnIsLoggerEnabled = value;
                    }
                }
            }
        }

        //        /// <summary>
        //        /// Log that the execution path is entering a method
        //        /// </summary>
        //        /// <param name="info">An optional array of objects providing additional data</param>
        //        /// <param name="file">Do not use this</param>
        //        /// <param name="method">Do not use this</param>
        //        /// <param name="line">Do not use this</param>
        //        public static void Enter
        //        (
        //            object[] info = null,
        //            [CallerFilePath] string file = "",
        //            [CallerMemberName] string method = "",
        //            [CallerLineNumber] int line = 0
        //        )
        //        {
        //            writeLog(info, file, method, line, "Entering ");
        //        }

        /// <summary>
        /// Log that the execution path is entering a method
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Entering
        (
            string info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
        )
        {
            WriteLog(("Entering " + info).Yield(), file ?? string.Empty, method, line, LogLevel.Debug);
        }

        /// <summary>
        /// Log that the execution path is entering a method
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Exit
        (
            string info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
        )
        {
            WriteLog(("Exiting " + info).Yield(), file ?? string.Empty, method, line, LogLevel.Debug);
        }

        /// <summary>
        /// Log that an error occoured
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Error
            (
            object[] info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info?.Select(x => x.ToString()), file ?? String.Empty, method, line, LogLevel.Error);
        }

        /// <summary>
        /// Log that an error occured
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Error
            (
            object info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info?.ToString().Yield(), file ?? string.Empty, method, line, LogLevel.Error);
        }

        /// <summary>
        /// Log something that could help with debug
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Debug
        (
            object info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
        )
        {
            WriteLog(info?.ToString().Yield(), file ?? string.Empty, method, line, LogLevel.Debug);
        }

        /// <summary>
        /// Log a trace message (this should be off by default)
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Trace
        (
            string info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
        )
        {
            WriteLog(info.Yield(), file ?? string.Empty, method, line, LogLevel.Trace);
        }

        /// <summary>
        /// Log a trace message (this should be off by default)
        /// </summary>
        /// <param name="exception">the actual exception</param>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Trace
        (
            Exception exception,
            object info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
        )
        {
            WriteLog(EnumerableExtensions.ToEnumerable(exception.ToString(), info?.ToString() ?? string.Empty), file ?? string.Empty, method, line, LogLevel.Trace);
        }

        /// <summary>
        /// Log an exception has occured
        /// </summary>
        /// <param name="exception">Exception to log.</param>
        /// <param name="message">Message of Exception</param>
        [Obsolete("Use NLog instead: private static readonly Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Exception(Exception exception, string message = "")
        {
            if (!IsLoggerEnabled)
                return;
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            WriteLog(
                EnumerableExtensions.ToEnumerable(message, exception.ToString(), exception.StackTrace),
                exception.Source,
                exception.TargetSite.Name,
                (new StackTrace(exception, true)).GetFrame(0).GetFileLineNumber(),
                LogLevel.Fatal);
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Warning
            (
            object[] info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info?.Select(x => x.ToString()), file ?? string.Empty, method, line, LogLevel.Warn);
        }

        /// <summary>
        /// Log a warning
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Warning
            (
            object info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info?.ToString().Yield(), file ?? string.Empty, method, line, LogLevel.Warn);
        }

        /// <summary>
        /// Log some info
        /// </summary>
        /// <param name="info">An optional object providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Info
            (
            object[] info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info?.Select(x => x.ToString()), file ?? string.Empty, method, line, LogLevel.Info);
        }

        /// <summary>
        /// Log some info
        /// </summary>
        /// <param name="info">An optional array of objects providing additional data</param>
        /// <param name="file">Do not use this</param>
        /// <param name="method">Do not use this</param>
        /// <param name="line">Do not use this</param>
        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void Info
            (
            string info = null,
            [CallerFilePath] string file = "",
            [CallerMemberName] string method = "",
            [CallerLineNumber] int line = 0
            )
        {
            WriteLog(info.Yield(), file ?? string.Empty, method, line, LogLevel.Info);
        }

        public enum LogLevel
        {
            Off = -1,
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        private static void WriteLog(IEnumerable<string> info, string file, string method, int line, LogLevel loglevel)
        {
            if (!IsLoggerEnabled)
                return;

            Stopwatch sw = Stopwatch.StartNew();
            //TODO: Add timestamp to logs

            StringBuilder sbdTimeStamper = new StringBuilder(loglevel + "\t" + (file.SplitNoAlloc(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).LastOrDefault() ?? string.Empty)
                                                             + '.' + method + ':' + line);

            if (info != null)
            {
                bool blnItemAdded = false;
                sbdTimeStamper.Append(' ');
                foreach (string time in info)
                {
                    if (string.IsNullOrWhiteSpace(time))
                        continue;
                    blnItemAdded = true;
                    sbdTimeStamper.Append(time + ", ");
                }

                sbdTimeStamper.Length -= blnItemAdded ? 2 : 1;
            }

            sw.TaskEnd("makeentry");

            string strTimeStamp = sbdTimeStamper.ToString();
            lock (s_LogWriterLock)
                _logWriter?.WriteLine(strTimeStamp);
            sw.TaskEnd("filewrite");
            System.Diagnostics.Trace.WriteLine(strTimeStamp);
            sw.TaskEnd("screenwrite");
        }

        [Obsolete("Use NLog instead: private static Logger Log = NLog.LogManager.GetCurrentClassLogger();")]
        public static void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (IsLoggerEnabled)
            {
                lock (s_LogWriterLock)
                    _logWriter?.WriteLine("First chance exception: " + e?.Exception);
            }
        }
    }
}
