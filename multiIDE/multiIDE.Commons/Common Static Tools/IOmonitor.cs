using System;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace multiIDE
{
    public static class IOmonitor
    {
        public static void IdleForLock(object locking)
        {
            if (!Monitor.IsEntered(locking))
                while (!Monitor.TryEnter(locking))
                {
                    Application.DoEvents();
                    Thread.Yield();
                }
        }

        public static void IdleForLock(object locking, out bool lockTaken)
        {
            lockTaken = Monitor.IsEntered(locking);
            while (!lockTaken)
            {
                Monitor.TryEnter(locking, ref lockTaken);
                Application.DoEvents();
                Thread.Yield();
            }
        }

        public static void IdleForLock(this IInputDevice inputDevice)
        {
            if (!Monitor.IsEntered(inputDevice.Locking))
                while (!Monitor.TryEnter(inputDevice.Locking))
                {
                    Application.DoEvents();
                    Thread.Yield();
                }
        }

        public static void IdleForLock(this IInputDevice inputDevice, out bool lockTaken)
        {
            lockTaken = Monitor.IsEntered(inputDevice.Locking);
            while (!lockTaken)
            {
                Monitor.TryEnter(inputDevice.Locking, ref lockTaken);
                Application.DoEvents();
                Thread.Yield();
            }
        }

        public static void IdleForLock(this IOutputDevice outputDevice)
        {
            if (!Monitor.IsEntered(outputDevice.Locking))
                while (!Monitor.TryEnter(outputDevice.Locking))
                {
                    Application.DoEvents();
                    Thread.Yield();
                }
        }

        public static void IdleForLock(this IOutputDevice outputDevice, out bool lockTaken)
        {
            lockTaken = Monitor.IsEntered(outputDevice.Locking);
            while (!lockTaken)
            {
                Monitor.TryEnter(outputDevice.Locking, ref lockTaken);
                Application.DoEvents();
                Thread.Yield();
            }
        }

        public static bool TryLock(this IInputDevice inputDevice)
        {
            if (!Monitor.IsEntered(inputDevice.Locking))
                return Monitor.TryEnter(inputDevice.Locking);
            else
                return true;
        }

        public static void TryLock(this IInputDevice inputDevice, out bool lockTaken)
        {
            lockTaken = Monitor.IsEntered(inputDevice.Locking);
            if (!lockTaken)
                Monitor.TryEnter(inputDevice.Locking, ref lockTaken);
        }

        public static bool TryLock(this IOutputDevice outputDevice)
        {
            if (!Monitor.IsEntered(outputDevice.Locking))
                return Monitor.TryEnter(outputDevice.Locking);
            else
                return true;
        }

        public static void TryLock(this IOutputDevice outputDevice, out bool lockTaken)
        {
            lockTaken = Monitor.IsEntered(outputDevice.Locking);
            if (!lockTaken)
                Monitor.TryEnter(outputDevice.Locking, ref lockTaken);
        }

        public static void Unlock(object locking)
        {
            if (Monitor.IsEntered(locking))
                Monitor.Exit(locking);
        }

        public static void Unlock(this IInputDevice inputDevice)
        {
            if (Monitor.IsEntered(inputDevice.Locking))
                Monitor.Exit(inputDevice.Locking);
        }

        public static void Unlock(this IOutputDevice outputDevice)
        {
            if (Monitor.IsEntered(outputDevice.Locking))
                Monitor.Exit(outputDevice.Locking);
        }

        public static void IdleWhile(Func<bool> condition)
        {
            while (condition())
            {
                Application.DoEvents();
                Thread.Yield();
            }
        }

        public static void IdleUntil(Func<bool> condition)
        {
            do
            {
                Application.DoEvents();
                Thread.Yield();
            } while (!condition());
        }

        public static bool IsLockingRequiredForInitialize(this IInputDevice inputDevice)
        {
            return inputDevice.GetType().GetMethod("Initialize").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? true;
        }

        public static bool IsLockingRequiredForDispose(this IInputDevice inputDevice)
        {
            return inputDevice.GetType().GetMethod("Dispose").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? true;
        }

        public static bool IsLockingRequiredForInput(this IInputDevice inputDevice)
        {
            return inputDevice.GetType().GetMethod("Input").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? false;
        }

        public static bool IsLockingRequiredForInitialize(this IOutputDevice outputDevice)
        {
            return outputDevice.GetType().GetMethod("Initialize").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? true;
        }

        public static bool IsLockingRequiredForDispose(this IOutputDevice outputDevice)
        {
            return outputDevice.GetType().GetMethod("Dispose").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? true;
        }

        public static bool IsLockingRequiredForOutput(this IOutputDevice outputDevice)
        {
            return outputDevice.GetType().GetMethod("Output").GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? false;
        }
    }
}
