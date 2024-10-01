using UnityEngine;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using afjk.RuntimeLogger.Utilities;
using UnityEditor;
using UnityEngine.Networking;
using afjk.RuntimeLogger.Editor;

public class DebugLogServerMenu
{
    [MenuItem("DebugLogServer/Start Server")]
    public static void StartServer()
    {
        DebugLogServer.StartServer();
    }

    [MenuItem("DebugLogServer/Stop Server")]
    public static void StopServer()
    {
        DebugLogServer.StopServer();
    }
}

namespace afjk.RuntimeLogger.Editor
{
    public class DebugLogServer
    {
        private static HttpListener listener;
        private static Thread listenerThread;
        private static int port = 8085;

        static DebugLogServer()
        {
            StartServer();
        }

        public static void StartServer()
        {
            if (listener != null && listener.IsListening)
                return;

            listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/log/");
            try
            {
                listener.Start();
                listenerThread = new Thread(HandleIncomingConnections);
                listenerThread.IsBackground = true;
                listenerThread.Start();

                Debug.Log($"[DebugLogServer] Started on port {port}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DebugLogServer] Failed to start server: {ex.Message}");
            }
        }

        public static void StopServer()
        {
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }

            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }

        private static void HandleIncomingConnections()
        {
            while (listener != null && listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();
                    ProcessRequest(context);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[DebugLogServer] Exception: {ex.Message}");
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    var body = reader.ReadToEnd();

                    // フォームデータを解析
                    var formData = ParseFormData(body);

                    if (formData.TryGetValue("logString", out string logString) &&
                        formData.TryGetValue("stackTrace", out string stackTrace) &&
                        formData.TryGetValue("logType", out string logType))
                    {
                        // Unityのメインスレッドでログを出力
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            if (logType == "Error" || logType == "Exception")
                            {
                                Debug.LogError($"[Remote] {logString}\n{stackTrace}");
                            }
                            else if (logType == "Warning")
                            {
                                Debug.LogWarning($"[Remote] {logString}");
                            }
                            else
                            {
                                Debug.Log($"[Remote] {logString}");
                            }
                        });
                    }

                    // レスポンスを返す
                    byte[] buffer = Encoding.UTF8.GetBytes("OK");
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            context.Response.OutputStream.Close();
        }

        private static Dictionary<string, string> ParseFormData(string formData)
        {
            var dict = new Dictionary<string, string>();
            string[] pairs = formData.Split('&');
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    string key = UnityWebRequest.UnEscapeURL(kv[0]);
                    string value = UnityWebRequest.UnEscapeURL(kv[1]);
                    dict[key] = value;
                }
            }
            return dict;
        }
    }
}