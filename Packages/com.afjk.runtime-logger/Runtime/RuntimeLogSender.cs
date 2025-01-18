using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine;

namespace com.afjk.RuntimeLogger
{
    public class RuntimeLogSender
    {
        public string ServerUrl { get; set; } = "localhost";
        public int ServerPort { get; set; } = 8081;
        private bool isLogging = true; // ログ出力の制御フラグ

        // フィルタリングの条件を表すデリゲート
        public delegate bool LogFilter(string logString, string stackTrace, LogType type);

        private LogFilter userFilter;

        // デフォルトのフィルター。com.afjk.runtime-loggerパッケージ内でのログは送信しない。
        private LogFilter defaultFilter = (logString, stackTrace, type) => !stackTrace.Contains("com.afjk.runtime-logger");

        // ログのバッファリング
        private Queue<(string logString, string stackTrace, LogType type)> logQueue =
            new Queue<(string, string, LogType)>();

        private UdpClient udpClient;

        public RuntimeLogSender()
        {
            udpClient = new UdpClient();
        }

        public void SetUserFilter(LogFilter filter)
        {
            userFilter = filter;
        }

        private bool CombinedFilter(string logString, string stackTrace, LogType type)
        {
            return defaultFilter(logString, stackTrace, type) && (userFilter == null || userFilter(logString, stackTrace, type));
        }

        public void StartLogging()
        {
            isLogging = true;
        }

        public void StopLogging()
        {
            isLogging = false;
        }

        public void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (isLogging && CombinedFilter(logString, stackTrace, type))
            {
                // ログをキューに保存
                logQueue.Enqueue((logString, stackTrace, type));
            }
        }

        public void SendLogs()
        {
            Task.Run(async () =>
            {
                while (isLogging)
                {
                    if (logQueue.Count > 0)
                    {
                        var log = logQueue.Dequeue();
                        string logMessage =
                            $"logString: {UnityWebRequest.EscapeURL(log.logString)}, stackTrace: {UnityWebRequest.EscapeURL(log.stackTrace)}, logType: {UnityWebRequest.EscapeURL(log.type.ToString())}";
                        byte[] data = Encoding.UTF8.GetBytes(logMessage);

                        udpClient.Send(data, data.Length, ServerUrl, ServerPort);
                    }

                    // Wait for a short amount of time before the next iteration.
                    await Task.Delay(100);
                }
            });
        }
    }
}