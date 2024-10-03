using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using afjk.RuntimeLogger.Editor;
using afjk.RuntimeLogger.Utilities;
using UnityEditor;
using UnityEngine.Networking;

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
        private static UdpClient udpClient;
        private static Thread listenerThread;
        private static int port = 8085;

        static DebugLogServer()
        {
            StartServer();
        }

        public static void StartServer()
        {
            if (udpClient != null)
                return;

            udpClient = new UdpClient(port);
            listenerThread = new Thread(HandleIncomingConnections);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Debug.Log($"[DebugLogServer] Started on port {port}");
        }

        public static void StopServer()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            if (listenerThread != null)
            {
                listenerThread.Abort();
                listenerThread = null;
            }
        }

        private static void HandleIncomingConnections()
        {
            while (udpClient != null)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string logMessage = Encoding.UTF8.GetString(data);
                    // ログメッセージからLogType, logString, stackTraceを抽出
                    string[] parts = logMessage.Split(',');
                    string logString = parts[0].Substring(parts[0].IndexOf(":") + 1).Trim();
                    string stackTrace = parts[1].Substring(parts[1].IndexOf(":") + 1).Trim();
                    string logTypeString = parts[2].Substring(parts[2].IndexOf(":") + 1).Trim();
                    logString = UnityWebRequest.UnEscapeURL(logString);
                    stackTrace = UnityWebRequest.UnEscapeURL(stackTrace);
                    logTypeString = UnityWebRequest.UnEscapeURL(logTypeString);
                    
                    Debug.Log($"[DBG] {logMessage}");
                    

                    if (Enum.TryParse(logTypeString, out LogType logType))
                    {
                        // Unityのメインスレッドでログを出力
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            switch (logType)
                            {
                                case LogType.Log:
                                    Debug.Log($"[Remote] {logString}\n{stackTrace}");
                                    break;
                                case LogType.Warning:
                                    Debug.LogWarning($"[Remote] {logString}\n{stackTrace}");
                                    break;
                                case LogType.Error:
                                    Debug.LogError($"[Remote] {logString}\n{stackTrace}");
                                    break;
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError($"[DebugLogServer] Invalid LogType: {logTypeString}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[DebugLogServer] Exception: {ex.Message}");
                }
            }
        }
    }
}