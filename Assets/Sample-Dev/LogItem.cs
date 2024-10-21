using TMPro;
using UnityEngine;

namespace com.afjk.RuntimeLogger
{
    public class LogItem : MonoBehaviour
    {
        [SerializeField] private bool showTime = true;
        [SerializeField] private TMP_Text text;

        public void AddText(string log, LogType type)
        {
            if (showTime)
            {
                log = $"[{System.DateTime.Now:HH:mm:ss}] {log}";
            }

            string logTypeString = type.ToString();
            string coloredLogType = $"<color={GetColorForLogType(type)}>●</color>";
            text.text = $"{coloredLogType} {log}";
        }

        private string GetColorForLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    return "white";
                case LogType.Warning:
                    return "yellow";
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return "red";
                default:
                    return "white";
            }
        }
    }
}