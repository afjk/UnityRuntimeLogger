using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;

namespace afjk.RuntimeLogger
{
    public class DebugLogSender : MonoBehaviour
    {
        public string serverUrl = "localhost";
        public int serverPort = 8085;
        private bool isLogging = true; // ログ出力の制御フラグ

        // ログのバッファリング
        private Queue<(string logString, string stackTrace, LogType type)> logQueue = new Queue<(string, string, LogType)>();

        private UdpClient udpClient;

        void Start()
        {
            udpClient = new UdpClient();
        }

        /// <summary>
        /// serverUrlを設定するメソッド
        /// </summary>
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

        /// <summary>
        /// ログ出力を開始するメソッド
        /// </summary>
        public void StartLogging()
        {
            isLogging = true;
            // キュー内のログを送信
            if (logQueue.Count > 0)
            {
                StartCoroutine(SendBufferedLogs());
            }
        }

        /// <summary>
        /// ログ出力を停止するメソッド
        /// </summary>
        public void StopLogging()
        {
            isLogging = false;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (isLogging)
            {
                // ログを即座に送信
                StartCoroutine(SendLogToServer(logString, stackTrace, type));
            }
            else
            {
                // ログをキューに保存
                logQueue.Enqueue((logString, stackTrace, type));
            }
        }

        /// <summary>
        /// バッファされたログをサーバーに送信するコルーチン
        /// </summary>
        IEnumerator SendBufferedLogs()
        {
            while (logQueue.Count > 0)
            {
                var log = logQueue.Dequeue();
                yield return StartCoroutine(SendLogToServer(log.logString, log.stackTrace, log.type));
            }
        }

        /// <summary>
        /// ログをサーバーに送信するコルーチン
        /// </summary>
        IEnumerator SendLogToServer(string logString, string stackTrace, LogType type)
        {
            string logMessage = $"logString: {UnityWebRequest.EscapeURL(logString)}, stackTrace: {UnityWebRequest.EscapeURL(stackTrace)}, logType: {UnityWebRequest.EscapeURL(type.ToString())}";
            byte[] data = Encoding.UTF8.GetBytes(logMessage);

            udpClient.Send(data, data.Length, serverUrl, serverPort);

            yield return null;
        }
    }
}