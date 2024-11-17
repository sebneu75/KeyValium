using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    /*
    
        using System.Runtime.InteropServices;   //GuidAttribute
        using System.Reflection;                //Assembly
        using System.Threading;                 //Mutex
        using System.Security.AccessControl;    //MutexAccessRule
        using System.Security.Principal;        //SecurityIdentifier

        static void Main(string[] args)
        {
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid =
                ((GuidAttribute)Assembly.GetExecutingAssembly().
                    GetCustomAttributes(typeof(GuidAttribute), false).
                        GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format( "Global\\{{{0}}}", appGuid );

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule =
                new MutexAccessRule( new SecurityIdentifier( WellKnownSidType.WorldSid
                                                           , null)
                                   , MutexRights.FullControl
                                   , AccessControlType.Allow
                                   );
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

           // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                // edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        // mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process,
                        // it will still get acquired
                        hasHandle = true;
                    }

                    // Perform your work here.
                }
                finally
                {
                    // edited by acidzombie24, added if statement
                    if(hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }    

    */
    
    internal class MutexLock : ILockable
    {
        /// <summary>
        /// Name for the MainMutex (do not change)
        /// </summary>
        const string MainMutexName = @"Global\KeyValium-MainMutex-{fa581e28-185b-46fe-bfa9-bc02c79dc85a}";

        /// <summary>
        /// Format string for Mutexname (do not change)
        /// </summary>
        const string MutexFormatString = @"Global\KeyValium-Mutex-{0:B}";

        #region Constructor

        internal MutexLock(Database db)
        {
            MainMutex = new Mutex(false, MainMutexName);

            Timeout = db.Options.LockTimeout;
        }

        #endregion

        #region Variables

        internal readonly Mutex MainMutex;

        internal readonly object _mainlock = new object();

        internal Mutex Mutex;

        internal readonly object _lock = new object();

        internal string MutexName;

        internal readonly int Timeout;

        #endregion

        #region ILockable implementation

        public void Lock()
        {
            Perf.CallCount();

            ValidateLock(false);

            try
            {
                if (!Mutex.WaitOne(Timeout))
                {
                    throw new TimeoutException("Could not aquire lock within timeout.");
                }
            }
            catch (AbandonedMutexException ex)
            {
                // Mutex will still get acquired
                Logger.LogError(LogTopics.Lock, ex, "Abandoned Mutex.");
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Lock, ex, "Error while locking.");
                throw;
            }
        }

        public void Unlock()
        {
            Perf.CallCount();

            ValidateLock(true);

            try
            {
                Mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Lock, ex, "Error while locking.");
                throw;
            }
        }

        public void ValidateLock(bool expected)
        {
            Perf.CallCount();

            // cannot be done with mutexes so do nothing            
        }

        public void LockForCreation()
        {
            Perf.CallCount();

            ValidateCreationLock(false);

            try
            {
                if (!MainMutex.WaitOne(Timeout))
                {
                    throw new TimeoutException("Could not aquire lock within timeout.");
                }
            }
            catch (AbandonedMutexException ex)
            {
                // Mutex will still get acquired
                Logger.LogError(LogTopics.Lock, ex, "Abandoned MainMutex.");
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Lock, ex, "Error while locking.");
                throw;
            }
        }

        public void UnlockForCreation()
        {
            Perf.CallCount();

            ValidateCreationLock(true);

            try
            {
                MainMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Logger.LogError(LogTopics.Lock, ex, "Error while locking.");
                throw;
            }
        }

        public void ValidateCreationLock(bool expected)
        {
            Perf.CallCount();

            // cannot be done with mutexes so do nothing            
        }

        public void CreateLock(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                // must not be empty for MutexLock
                throw new KeyValiumException(ErrorCodes.InternalError, "LockGuid is empty.");
            }

            // make name
            MutexName = string.Format(MutexFormatString, guid);

            // create mutex
            Mutex = new Mutex(false, MutexName);

            Logger.LogInfo(LogTopics.Lock, "Mutex created: {0}", MutexName);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Mutex?.Dispose();
            Mutex = null;

            MainMutex?.Dispose();
        }

        #endregion
    }
}
