using System;
using System.Collections.Generic;
using Winner;

namespace Configuration
{
    public class EventHandleArgs
    {
        /// <summary>
        /// 事件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 触发源
        /// </summary>
        public object Sender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string,string> Forms { get; set; }

   
    }
    public class EventHandle
    {
        /// <summary>
        /// 是否异步执行
        /// </summary>
        public bool IsAsync { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public Action<EventHandleArgs> Action { get; set; }
    }
    static public class EventManager
    {
     
        private static IDictionary<string,IList<EventHandle>> Handles=new Dictionary<string, IList<EventHandle>>();

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="className"></param>
        /// <param name="method"></param>
        /// <param name="isAsync"></param>
        public static void Register(string eventName, string className,string method,bool isAsync)
        {
            var obj = Creator.Get<Winner.Creation.IFactory>().Get<object>(className);
            if (obj == null)
                obj = CreateClass(className);
            if (obj != null)
            {
                var action= (Action<EventHandleArgs>)Delegate.CreateDelegate(typeof(Action<EventHandleArgs>), obj, method);
                var eventHandle = new EventHandle { Action = action, IsAsync = isAsync };
                Register(eventName, eventHandle);
            }
        }
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public static void Register(string eventName, EventHandle handle)
        {
           if(!Handles.ContainsKey(eventName))
               Handles.Add(eventName,new List<EventHandle>());
            Handles[eventName].Add(handle);
        }

        private static object RegisterLocker = new object();
        private static IDictionary<string, object> ClientKeys = new Dictionary<string, object>();
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        /// <param name="key"></param>
        public static void Register(string eventName, EventHandle handle,string key)
        {
            if(ClientKeys.ContainsKey(key))
                return;
            lock (RegisterLocker)
            {
                if (ClientKeys.ContainsKey(key))
                    return;
                ClientKeys.Add(key,new object());
            }
            if (!Handles.ContainsKey(eventName))
                Handles.Add(eventName, new List<EventHandle>());
            Handles[eventName].Add(handle);
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="args"></param>
        public static void Execute(EventHandleArgs args)
        {
            if(args==null || string.IsNullOrWhiteSpace(args.Name))
                return;
            if (Handles.ContainsKey(args.Name) && Handles[args.Name] !=null)
            {
                foreach (var eventHandle in Handles[args.Name])
                {
                 if(eventHandle.Action==null)
                        continue;
                    if (eventHandle.IsAsync)
                    {
                        var task = new System.Threading.Tasks.Task(() => { eventHandle.Action(args); });
                        task.Start();
                    }
                    else
                    {
                        eventHandle.Action(args);
                    }
                }
            }
        }
        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private static object CreateClass(string className)
        {
            var t = Type.GetType(className);
            if (t == null) return null;
            return Activator.CreateInstance(t);
        }
    }
}
