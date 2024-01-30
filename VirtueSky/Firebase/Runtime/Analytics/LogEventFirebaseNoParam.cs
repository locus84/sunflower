using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.FirebaseTraking
{
    [CreateAssetMenu(menuName = "Firebase Analytic/Log Event No Param",
        fileName = "log_event_firebase_no_param")]
    public class LogEventFirebaseNoParam : ScriptableObject
    {
        [Space] [HeaderLine("Event Name")] [SerializeField]
        private string eventName;

        public void LogEvent()
        {
            if (!Application.isMobilePlatform) return;
#if VIRTUESKY_FIREBASE_ANALYTIC
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
#endif
        }
    }
}