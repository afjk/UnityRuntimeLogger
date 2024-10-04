using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using afjk.RuntimeLogger.Utilities;
using UnityEngine.Networking;


namespace com.afjk.RuntimeLogger.Editor
{
    public class RuntimeLogReceiver
    {
        private UdpClient udpClient;
        private Thread listenerThread;
        public int port = 8081;

        
        private bool shouldStop = false;

        public void StartReceiving()
        {
            if (udpClient != null)
                return;

            shouldStop = false;
            udpClient = new UdpClient(port);
            listenerThread = new Thread(HandleIncomingConnections);
            listenerThread.IsBackground = true;
            listenerThread.Start();

            Debug.Log($"Start receiving on port {port}");
        }

        public void StopReceiving()
        {
            if (udpClient != null)
            {
                shouldStop = true;
                udpClient.Close();
                udpClient = null;
            }

            listenerThread = null;
            Debug.Log($"Stop receiving");
        }

        private void HandleIncomingConnections()
        {
            while (!shouldStop && udpClient != null)
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
                    stackTrace = ConvertStackTrace(stackTrace);
                    string ipAddress = remoteEP.Address.ToString();
                    stackTrace += $"\n---- from:{ipAddress} ----\n\n\n\n\n";
                    logTypeString = UnityWebRequest.UnEscapeURL(logTypeString);
                    
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
                                case LogType.Exception:
                                case LogType.Assert:
                                    Debug.LogError($"[Remote] {logString}\n{stackTrace}");
                                    break;
                                default:
                                    Debug.Log($"[Remote] {logString}\n{stackTrace}\nLogType:{logType}");
                                    break;
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError($"Invalid LogType: {logTypeString}");
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        Debug.LogError($"Exception: {ex.Message}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception: {ex.Message}");
                }
            }
        }
        string ConvertStackTrace(string stackTrace)
        {
            var lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var match = System.Text.RegularExpressions.Regex.Match(lines[i], @"(.+) \(at (.+):(\d+)\)");
                if (match.Success)
                {
                    string method = match.Groups[1].Value;
                    string filePath = match.Groups[2].Value;

                    int assetsIndex = filePath.IndexOf("Assets");
                    if (assetsIndex >= 0)
                    {
                        filePath = filePath.Substring(assetsIndex);
                    }

                    string line = match.Groups[3].Value;
                    lines[i] = $"{method} (at <a href=\"{filePath}\" line=\"{line}\">{filePath}:{line}</a>)";
                }
            }
            return string.Join("\n", lines);
        }
    }
}
