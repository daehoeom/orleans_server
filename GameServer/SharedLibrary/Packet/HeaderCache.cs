using System;
using System.Reflection;
using SharedLibrary.Packet.Base;

namespace SharedLibrary.Packet
{
    public static class ResponseHeaderCache<T>
        where T : class
    {
        public static readonly PacketHeaderType HeaderType =
            typeof(T).GetCustomAttribute<ResponseAttribute>()?.HeaderType
            ?? throw new InvalidOperationException($"[ResponseHeader] 어트리뷰트가 없습니다: {typeof(T).Name}");
    }

    public static class NotifyHeaderCache<T>
        where T : class
    {
        public static readonly PacketHeaderType HeaderType =
            typeof(T).GetCustomAttribute<NotifyAttribute>()?.HeaderType
            ?? throw new InvalidOperationException($"[NotifyHeader] 어트리뷰트가 없습니다: {typeof(T).Name}");
    }
}