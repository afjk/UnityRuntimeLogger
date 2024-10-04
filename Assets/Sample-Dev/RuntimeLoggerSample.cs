using UnityEngine;

namespace com.afjk.RuntimeLogger
{
    
    public class RuntimeLoggerSample : MonoBehaviour
    {
        private RuntimeLogSender _runtimeLogSender = new RuntimeLogSender();
        public string serverUrl = "localhost";
        public int serverPort = 8081;
        
        void Start()
        {
            _runtimeLogSender.ServerUrl = serverUrl;
            _runtimeLogSender.ServerPort = serverPort;
            _runtimeLogSender.StartLogging();
        }

        void OnEnable()
        {
            Application.logMessageReceived += _runtimeLogSender.HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= _runtimeLogSender.HandleLog;
        }

        void Update()
        {
            _runtimeLogSender.SendLogs();
        }
    }
}