using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// SessionManager provides an abstraction and encapsulation of the connection to a Hansoft database from a Hansoft SDK client program.
    /// The class is a singleton, meaning that you only can connect to one Hansoft database at the time.
    /// 
    /// When using the SessionManager to manage a connection to a Hansoft server/database, the lifecycle is as follows:
    /// 1.  Call Intialize/5 to set the connection parameters
    /// 2a. Call Connect/0 to connect to Hansoft without listening to event notifications, or,
    /// 2b. Call Connect/2 to connect to Hansoft while listening to event notifications.
    /// 3.  Make calls to the Hansoft API with the openened session like this: SessionManager.Instance.Session.[SomeApiFunction].
    /// 4.  When done, close the session by calling CloseSession
    /// 
    /// If the connection is lost with Hansoft for some reason, e.g, you get a EHPMError.ConnectionLost return code when calling
    /// SessionProcess, you can attempt to reconnect with the previously specified settings by calling Reconnect.
    /// </summary>
    public class SessionManager
    {
        private static SessionManager instance = null;

        private HPMSdkCallbacks callbackHandler;
        private Semaphore callbackSemaphore;

        private string sdkUser;
        private string sdkUserPwd;
        private string server;
        private int port;
        private string database;

        HPMSdkSession hpmSession;

        /// <summary>
        /// Closes any current connections and opens a new connection to the specified Hansoft database.
        /// </summary>
        /// <param name="sdkUser">The SDK user name to connect as.</param>
        /// <param name="sdkUserPwd">The password of the SDK user.</param>
        /// <param name="server">The DNS name or IP address of the Hansoft server to connect to.</param>
        /// <param name="port">The port number for the Hansoft server.</param>
        /// <param name="database">The name of the database to access.</param>
        public static void Initialize(string sdkUser, string sdkUserPwd, string server, int port, string database)
        {
            if (instance != null)
                instance.CloseSession();
            instance = new SessionManager(sdkUser, sdkUserPwd, server, port, database);
        }

        private SessionManager(string sdkUser, string sdkUserPwd, string server, int port, string database)
        {
            this.sdkUser = sdkUser;
            this.sdkUserPwd = sdkUserPwd;
            this.server = server;
            this.port = port;
            this.database = database;
        }

        /// <summary>
        /// Connect to a Hansoft Server/Database with parameters specified in a preceding call to Intialize and
        /// create an Sdk session (retrieved through the Session property). The session will make change call backs
        /// as specified by the parameters to this function.
        /// </summary>
        /// <param name="callbackHandler">Subclass of HPMSdkCallbacks that specifies your callback handlers</param>
        /// <param name="callbackSemaphore">Semaphore that will be Released/Signaled when you need to call SessionProcess to process callbacks</param>
        /// <returns>True if the session could be created or one already exists, False otherwise.</returns>
        public bool Connect(HPMSdkCallbacks callbackHandler, Semaphore callbackSemaphore)
        {
            if (hpmSession == null)
            {
                this.callbackHandler = callbackHandler;
                this.callbackSemaphore = callbackSemaphore;
                return Connect();
            }
            else
                return true;
        }

        /// <summary>
        /// Connect to a Hansoft Server/Database with parameters specified in a preceding call to Intialize and
        /// create an Sdk session (retrieved through the Session property).
        /// </summary>
        /// <returns>True if the session could be created, False otherwise.</returns>
        public bool Connect()
        {
            try
            {
                hpmSession = HPMSdkSession.SessionOpen(server, port, database, sdkUser, sdkUserPwd, callbackHandler, callbackSemaphore, true, EHPMSdkDebugMode.Off, (IntPtr)null, 0, "", "", null);
            }
            catch (Exception)
            {
                hpmSession = null;
            }
            return hpmSession != null;
        }

        /// <summary>
        /// Reconnect to the Hansoft Server/Database specified in earlier calls to Initialize and Connect.
        /// </summary>
        /// <returns>True if the connection could be creates, False otherwise.</returns>
        public bool Reconnect()
        {
            return Connect();
        }

        /// <summary>
        /// Closes the current connection (if any) to a Hansoft server/database.
        /// </summary>
        public void CloseSession()
        {
            if (hpmSession != null)
            {
                HPMSdkSession.SessionDestroy(ref hpmSession);
                hpmSession = null;
            }
        }

        /// <summary>
        /// Returns the single instance of this class.
        /// </summary>
        public static SessionManager Instance
        {
            get
            {
                return instance;
            }
        }
        /// <summary>
        /// Indicates whether there is an active connection to a Hansoft server/database or not.
        /// </summary>
        public bool Connected
        {
            get
            {
                return hpmSession != null;
            }
        }

        /// <summary>
        /// Returns the current session to a Hansoft server/database.
        /// </summary>
        public static HPMSdkSession Session
        {
            get
            {
                if (instance != null)
                    return instance.hpmSession;
                else
                    return null;
            }
        }
    }
}
