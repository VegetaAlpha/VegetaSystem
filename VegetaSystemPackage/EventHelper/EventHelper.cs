
using System;
namespace VegetaSystem
{
    public class EventHelper
    {
        public static void RegisterListener<T>(ref T publisherEvent, T subscriberCallback) where T : Delegate
        {
            if (!IsListenerRegistered(publisherEvent, subscriberCallback))
            {
                publisherEvent = Delegate.Combine(publisherEvent, subscriberCallback) as T;
            }
        }

        public static void UnregisterListener<T>(ref T publisherEvent, T subscriberCallback) where T : Delegate
        {
            if (IsListenerRegistered(publisherEvent, subscriberCallback))
            {
                publisherEvent = Delegate.Remove(publisherEvent, subscriberCallback) as T;
            }
        }

        public static bool IsListenerRegistered<T>(T publisherEvent, T subscriberCallback) where T : Delegate
        {
            if (publisherEvent != null)
            {
                foreach (Delegate existingCallback in publisherEvent.GetInvocationList())
                {
                    if (existingCallback == (Delegate)subscriberCallback)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
