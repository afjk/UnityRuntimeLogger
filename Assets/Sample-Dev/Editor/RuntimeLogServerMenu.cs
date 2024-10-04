using UnityEditor;

namespace com.afjk.RuntimeLogger.Editor
{
    public class RuntimeLogServerMenu
    {
        private static RuntimeLogReceiver receiver = new RuntimeLogReceiver();
        
        [MenuItem("DebugLogServer/Start Server")]
        public static void StartServer()
        {
            receiver.StartServer();
        }

        [MenuItem("DebugLogServer/Stop Server")]
        public static void StopServer()
        {
            receiver.StopServer();
        }
    }
}