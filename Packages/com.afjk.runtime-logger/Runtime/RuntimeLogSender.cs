using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace com.afjk.RuntimeLogger
{
    public class RuntimeLogSender : MonoBehaviour
    {
        public string serverUrl = "localhost";
        public int serverPort = 8085;
        private bool isLogging = true; // ログ出力の制御フラグ

        // フィルタリングの条件を表すデリゲート
        public delegate bool LogFilter(string logString, string stackTrace, LogType type);
        LogFilter filter = (logString, stackTrace, type) => logString.StartsWith("[Remote]");
        
        // ログのバッファリング
        private Queue<(string logString, string stackTrace, LogType type)> logQueue = new Queue<(string, string, LogType)>();

        private UdpClient udpClient;

        void Start()
        {
            udpClient = new UdpClient();
            
            SendLogs();
        }

        public void SetServerUrl(string newUrl)
        {
            serverUrl = newUrl;
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }


        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        public void StartLogging()
        {
            isLogging = true;
        }

        public void StopLogging()
        {
            isLogging = false;
        }
        // フィルタリングの条件を登録するメソッド
        public void RegisterFilter(LogFilter newFilter)
        {
            filter = newFilter;
        }
        
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            
            if (isLogging && (filter == null || !filter(logString, stackTrace, type)))
            {
                // ログをキューに保存
                logQueue.Enqueue((logString, stackTrace, type));
            }
        }

        void SendLogs()
        {
            Task.Run(async () =>
            {
                while (isLogging)
                {
                    if (logQueue.Count > 0)
                    {
                        var log = logQueue.Dequeue();
                        string logMessage = $"logString: {UnityWebRequest.EscapeURL(log.logString)}, stackTrace: {UnityWebRequest.EscapeURL(log.stackTrace)}, logType: {UnityWebRequest.EscapeURL(log.type.ToString())}";
                        byte[] data = Encoding.UTF8.GetBytes(logMessage);

                        udpClient.Send(data, data.Length, serverUrl, serverPort);
                    }

                    // Wait for a short amount of time before the next iteration.
                    await Task.Delay(100);
                }
            });
        }
    }
}