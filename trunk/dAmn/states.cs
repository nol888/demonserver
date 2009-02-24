namespace dAmnSharp
{
    using System;

    public enum states
    {
        DISCONNECTED,
        DISCONNECTING,
        FETCHINGCOOKIE,
        CONNECTING,
        CONNECTED,
        HANDSHAKING,
        AUTHENTICATING,
        LOGGEDIN
    }
}

