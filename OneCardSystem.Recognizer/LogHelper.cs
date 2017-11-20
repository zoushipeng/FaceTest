using log4net;
using System.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading;

namespace OneCardSystem.Recognizer
{
    /// <summary>
    /// 辅助测试类 现在主要用来写日志的
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 日志文件路径
        /// </summary>
        private static string m_strLogPath = string.Empty;

        /// <summary>
        /// 程序的名字
        /// </summary>
        private static string m_strAppName = string.Empty;

        /// <summary>
        /// 写日志的锁
        /// </summary>
        private static object m_lock = new object();

        /// <summary>
        /// 记录消息Queue
        /// </summary>
        private static readonly ConcurrentQueue<string> _que;

        private static readonly ILog _log;
        private static readonly ManualResetEvent _mre;

        static LogHelper()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                string strPath = assembly.Location;
                if (strPath != null && strPath != string.Empty)
                {
                    m_strLogPath = strPath.Substring(0, strPath.LastIndexOf("\\")) + @"\";
                    m_strLogPath = m_strLogPath + "Log\\Recognizer\\";

                    if (!Directory.Exists(m_strLogPath))
                    {
                        Directory.CreateDirectory(m_strLogPath);
                    }
                }
                _que = new ConcurrentQueue<String>();
                _mre = new ManualResetEvent(false);
                _log = LogManager.GetLogger("App");
            }
        }

        /// <summary>
        /// 另一个线程记录日志，只在程序初始化时调用一次
        /// </summary>
        public static void Register()
        {
            Thread t = new Thread(new ThreadStart(WriteToFile));
            t.IsBackground = true;
            t.Start();
        }

        private static void WriteToFile()
        {
            while (true)
            {
                _mre.WaitOne();
                string msg;
                while (_que.Count > 0 && _que.TryDequeue(out msg))
                {
                    _log.Info(msg);
                    Console.WriteLine(msg);
                }
                _mre.Reset();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="strMsg">日志信息</param>
        public static void WriteLog(string strMsg, bool toconsole = false)
        {
            try
            {
                _que.Enqueue(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff ") + strMsg);
                _mre.Set();
                return;
                //if (toconsole || 1==1)
                //{
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff ") + strMsg);
                return;
                //}

                //return;
                lock (m_lock)
                {
                    string fileName = string.Empty;

                    fileName = m_strLogPath + m_strAppName + DateTime.Now.ToString("yyyy-MM-dd") + ".Log";

                    //fileName = m_strLogPath + m_strAppName + DateTime.Now.ToString("yyyy-MM-dd") + ".Log";
                    string writeInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff  ") + strMsg + "\r\n";
                    byte[] content;
                    content = Encoding.Default.GetBytes(writeInfo);
                    using (FileStream fsWriter = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        fsWriter.Seek(0, SeekOrigin.End);
                        fsWriter.Write(content, 0, content.Length);
                    }
                }
            }
            catch
            { }
        }
        public static void WriteLog(LogHelperStatu statu, string strMsg)
        {
            try
            {
                _que.Enqueue(strMsg);
                _mre.Set();
                return;

                string message = string.Format("{0} {1}", statu.ToString(), strMsg);
                //if (toconsole || 1==1)
                //{
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff ") + message);
                return;
                //}

                //return;
                lock (m_lock)
                {
                    string fileName = string.Empty;

                    fileName = m_strLogPath + m_strAppName + DateTime.Now.ToString("yyyy-MM-dd") + ".Log";

                    //fileName = m_strLogPath + m_strAppName + DateTime.Now.ToString("yyyy-MM-dd") + ".Log";
                    string writeInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff  ") + strMsg + "\r\n";
                    byte[] content;
                    content = Encoding.Default.GetBytes(writeInfo);
                    using (FileStream fsWriter = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        fsWriter.Seek(0, SeekOrigin.End);
                        fsWriter.Write(content, 0, content.Length);
                    }
                }
            }
            catch
            { }
        }
    }

    public enum LogHelperStatu
    {
        info = 0,
        error = 1,
        debug = 2
    }
}
