using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeBase.Shared
{
    public static class ProjectLogger
    {
	    // Измените галочку (и сам препроцессор) если логирование не нужно
#if UNITY_EDITOR
	    private const bool Enabled = true;
#else
	    private const bool Enabled = true;
#endif

	    private static readonly ILogger DefaultLogger = Debug.unityLogger;
	    private static readonly StringBuilder Builder = new(1024);

	    // LogInternal, public void Log... и сам источник
	    private const int SkipFramesLogInternal = 3;
	    // LogInternal, LogInternalBuilder, public void Log... и сам источник
	    private const int SkipFramesLogInternalBuilder = 4;
	    
	    private static string GetTypeByMember(string memberName, int skipFrames)
	    {
		    const string missFn = "MissFunction";
		    const string noAnyName = "NoAnyName";
		    
		    var stackFrame = new StackFrame(skipFrames, false);
		    var method = stackFrame.GetMethod();
		    
		    if (method == null)
			    return string.IsNullOrEmpty(memberName) ? memberName : missFn;

		    if (method.DeclaringType == null) return "NoType";

		    // В нормальной ситуации на этом моменте остановиться
		    var typeName = method.DeclaringType.Name;
		    
		    if (!string.IsNullOrEmpty(typeName))
			    return typeName;

		    if (string.IsNullOrEmpty(memberName))
			    return missFn;

		    // 2 альтернативный вариант
		    typeName = method.DeclaringType.FullName;

		    if (string.IsNullOrEmpty(typeName))
			    return noAnyName;

		    return typeName;
	    }
	    
		private static void LogInternal(object objMessage, LogType logType, 
			Object context, string memberNameManual, string memberName, int skipFrames = SkipFramesLogInternal)
		{
			if (!Enabled) return;

			Builder.Clear();
			
			var typeName = string.IsNullOrEmpty(memberNameManual) 
				? GetTypeByMember(memberName, skipFrames) : memberNameManual;
			
#if UNITY_EDITOR
			// objMessage = $"[<b><color=#{hexColor}>{typeName}</color></b>]: {message}";
			
			// Поддержка RichText есть только в UnityEditor, поэтому для Android.LogCat и Server это не имеет смысла
			var hexColor = ColorNameCache.GetRandomHexColor(typeName);
			Builder.Append("[<b><color=#");
			Builder.Append(hexColor);
			Builder.Append('>');
			Builder.Append(typeName);
			Builder.Append("</color></b>]: ");
#else
			// objMessage = $"[{typeName}]: {message}";

			Builder.Append('[');
			Builder.Append(typeName);
			Builder.Append("]: ");
#endif
			
			if (objMessage != null)
				Builder.Append(objMessage);
			else
				Builder.Append(memberName);
			
			objMessage = Builder.ToString();
			DefaultLogger.Log(logType, objMessage, context);
		}
		
		private static void LogInternal(StringBuilder builderMessage, LogType logType, 
			Object context, string memberNameManual, string memberName)
		{
			if (!Enabled) return;
			if (builderMessage == null) return;
			
			var objMessage = builderMessage.ToString();
			LogInternal(objMessage, logType, context, memberNameManual, memberName, SkipFramesLogInternalBuilder);
		}
		
		public static void Log(object message = null, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(message, LogType.Log, context, memberNameManual, memberName);
		}
		public static void LogWarning(object message, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(message, LogType.Warning, context, memberNameManual, memberName);
		}
		public static void LogError(object message, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(message, LogType.Error, context, memberNameManual, memberName);
		}
		public static void LogException(Exception exception, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(exception, LogType.Exception, context, memberNameManual, memberName);
		}
		
		public static void Log(StringBuilder builder, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(builder, LogType.Log, context, memberNameManual, memberName);
		}
		public static void LogWarning(StringBuilder builder, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(builder, LogType.Warning, context, memberNameManual, memberName);
		}
		public static void LogError(StringBuilder builder, Object context = null, 
			string memberNameManual = "", [CallerMemberName] string memberName = "")
		{
			LogInternal(builder, LogType.Error, context, memberNameManual, memberName);
		}
    }
}