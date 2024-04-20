﻿using ScreenTime.classes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenTime.Listeners
{
    internal class FocusChangeListener
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pdwProcessId);

        private readonly int sleepDelaySeconds = 1;

        public void Start()
        {
            Thread listener = new(() =>
            {
                int lastProcessId = -1;

                while (true)
                {
                    Thread.Sleep(sleepDelaySeconds * 1000);

                    IntPtr foregroundWindowHandle = GetForegroundWindow();
                    _ = GetWindowThreadProcessId(foregroundWindowHandle, out uint processId);
                    Process foregroundProcess = Process.GetProcessById((int)processId);

                    if (lastProcessId == processId)
                    {
                        OnFocusKeep(lastProcessId, sleepDelaySeconds);
                    }
                    else
                    {
                        OnFocusChange(lastProcessId, (int)processId);
                        lastProcessId = (int)processId;
                    }
                }
            })
            {
                IsBackground = true
            };

            listener.Start();
        }

        // Called when the periodical check occurs and the process is still the same
        private void OnFocusKeep(int processId, int keptFocusSeconds)
        {
            Process foregroundProcess = Process.GetProcessById(processId);
            ProcessModule? processModule = null;

            try
            {
                processModule = foregroundProcess.MainModule;
            }
            catch { }

            if (processModule == null) return;

            ScreenTimeApp screenTimeApp = ScreenTimeApp.CreateOrGetScreenTimeApp(foregroundProcess.ProcessName, processModule.FileName);
            screenTimeApp.IncreaseFocusSeconds((uint)keptFocusSeconds);
        }

        // Called when the focus changes from one process to another
        private void OnFocusChange(int lastProcessId, int processId)
        {
            Process foregroundProcess = Process.GetProcessById(processId);
            ProcessModule? processModule = null;

            try
            {
                processModule = foregroundProcess.MainModule;
            }
            catch { }

            if (processModule == null) return;

            ScreenTimeApp screenTimeApp = ScreenTimeApp.CreateOrGetScreenTimeApp(foregroundProcess.ProcessName, processModule.FileName);
            screenTimeApp.IncreaseFocusCount();
        }
    }
}
