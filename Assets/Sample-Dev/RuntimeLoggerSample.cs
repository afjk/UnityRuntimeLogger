using UnityEngine;

namespace com.afjk.RuntimeLogger.Sample
{
    
    public class RuntimeLoggerSample : MonoBehaviour
    {
        private RuntimeLogSender _runtimeLogSender = new RuntimeLogSender();
        public string serverUrl = "localhost";
        public int serverPort = 49784;
        
        void Start()
        {
            _runtimeLogSender.ServerUrl = serverUrl;
            _runtimeLogSender.ServerPort = serverPort;
            _runtimeLogSender.StartLogging();
            _runtimeLogSender.SendLogs();
        }

        void OnEnable()
        {
            Application.logMessageReceived += _runtimeLogSender.HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= _runtimeLogSender.HandleLog;
        }
    }
}