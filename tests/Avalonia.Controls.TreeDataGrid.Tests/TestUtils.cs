using System;
using System.Reflection;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public static class TestUtils
    {
        public static int GetEventSubscriberCount(object o, string eventName)
        {
            var fieldInfo = o.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo is null)
                throw new InvalidOperationException($"Event '{eventName}' not found.");

            var value = (MulticastDelegate?)fieldInfo.GetValue(o);
            return value?.GetInvocationList()?.Length ?? 0;
        }
    }
}
