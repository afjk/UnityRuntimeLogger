using UnityEditor;
using UnityEngine;

namespace com.afjk.RuntimeLogger.Editor
{
    public class RuntimeLogReceiverWindow : EditorWindow
    {
        private RuntimeLogReceiver receiver = new RuntimeLogReceiver();
        private bool isReceiving = false;

        [MenuItem("Tools/Runtime Log Receiver")]
        public static void ShowWindow()
        {
            GetWindow<RuntimeLogReceiverWindow>("Runtime Log Receiver");
        }

        private void OnGUI()
        {
            GUILayout.Label("Receiver Address", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUI.enabled = true;
            EditorGUILayout.TextField(GetLocalIPAddress());
            GUI.enabled = true;
            GUILayout.Label("Port:", GUILayout.Width(50));
            receiver.port = EditorGUILayout.IntField(receiver.port);
            GUILayout.EndHorizontal();

            if (!isReceiving)
            {
                if (GUILayout.Button("Start Receiving"))
                {
                    receiver.StartReceiving();
                    isReceiving = true;
                }
            }
            else
            {
                if (GUILayout.Button("Stop Receiving"))
                {
                    receiver.StopReceiving();
                    isReceiving = false;
                }
            }
        }

        private void OnDisable()
        {
            if (isReceiving)
            {
                receiver.StopReceiving();
            }
        }

        private string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}