using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace afjk.RuntimeLogger
{
    public class DebugLogSender : MonoBehaviour
    {
        public string serverUrl = "http://localhost:8085/log";
        private bool isLogging = true; // ログ出力の制御フラグ

        // ログのバッファリング
        private Queue<(string logString, string stackTrace, LogType type)> logQueue = new Queue<(string, string, LogType)>();

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
            WWWForm form = new WWWForm();
            form.AddField("logString", logString);
            form.AddField("stackTrace", stackTrace);
            form.AddField("logType", type.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[DebugLogSender] Failed to send log to server: {www.error}");
                }
            }
        }
    }
}