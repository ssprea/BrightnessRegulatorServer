using System.Runtime.InteropServices;

namespace ServerBrightnnes.Models;

public class WindowsAmdMonitorController : IMonitorController
{
    #region DllImport
        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(nint hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(nint hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        // Luminosità
        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorBrightness(nint handle, ref uint minimumBrightness, ref uint currentBrightness, ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetMonitorBrightness(nint handle, uint newBrightness);

        // Contrasto
        [DllImport("dxva2.dll", EntryPoint = "GetMonitorContrast")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorContrast(nint handle, ref uint minimumContrast, ref uint currentContrast, ref uint maxContrast);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorContrast")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetMonitorContrast(nint handle, uint newContrast);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitor")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitor(nint hMonitor);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, EnumMonitorsDelegate lpfnEnum, nint dwData);
        delegate bool EnumMonitorsDelegate(nint hMonitor, nint hdcMonitor, ref Rect lprcMonitor, nint dwData);
        #endregion

        private IReadOnlyCollection<MonitorInfo> Monitors { get; set; }

        public WindowsAmdMonitorController()
        {
            UpdateMonitors();
        }

        #region Luminosità
        public void SetBrightness(uint brightness)
        {
            SetBrightness(brightness, true);
        }

        private void SetBrightness(uint brightness, bool refreshMonitorsIfNeeded)
        {
            bool isSomeFail = false;
            foreach (var monitor in Monitors)
            {
                uint realNewValue = (monitor.MaxBrightness - monitor.MinBrightness) * brightness / 100 + monitor.MinBrightness;
                if (SetMonitorBrightness(monitor.Handle, realNewValue))
                {
                    monitor.CurrentBrightness = realNewValue;
                }
                else if (refreshMonitorsIfNeeded)
                {
                    isSomeFail = true;
                    break;
                }
            }

            if (refreshMonitorsIfNeeded && (isSomeFail || !Monitors.Any()))
            {
                UpdateMonitors();
                SetBrightness(brightness, false);
                return;
            }
        }

        public int GetBrightness()
        {
            if (!Monitors.Any())
            {
                return -1;
            }
            return (int)Monitors.Average(d => d.CurrentBrightness);
        }
        #endregion

        #region Contrasto
        public void SetContrast(uint contrast)
        {
            SetContrast(contrast, true);
        }

        private void SetContrast(uint contrast, bool refreshMonitorsIfNeeded)
        {
            bool isSomeFail = false;
            foreach (var monitor in Monitors)
            {
                uint realNewValue = (monitor.MaxContrast - monitor.MinContrast) * contrast / 100 + monitor.MinContrast;
                if (SetMonitorContrast(monitor.Handle, realNewValue))
                {
                    monitor.CurrentContrast = realNewValue;
                }
                else if (refreshMonitorsIfNeeded)
                {
                    isSomeFail = true;
                    break;
                }
            }

            if (refreshMonitorsIfNeeded && (isSomeFail || !Monitors.Any()))
            {
                UpdateMonitors();
                SetContrast(contrast, false);
                return;
            }
        }

        public int GetContrast()
        {
            if (!Monitors.Any())
            {
                return -1;
            }
            return (int)Monitors.Average(d => d.CurrentContrast);
        }
        #endregion

        private void UpdateMonitors()
        {
            DisposeMonitors(Monitors);

            var monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(nint.Zero, nint.Zero, (nint hMonitor, nint hdcMonitor, ref Rect lprcMonitor, nint dwData) =>
            {
                uint physicalMonitorsCount = 0;
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref physicalMonitorsCount))
                {
                    return true;
                }

                var physicalMonitors = new PHYSICAL_MONITOR[physicalMonitorsCount];
                if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorsCount, physicalMonitors))
                {
                    return true;
                }

                foreach (PHYSICAL_MONITOR physicalMonitor in physicalMonitors)
                {
                    uint minBrightness = 0, currentBrightness = 0, maxBrightness = 0;
                    uint minContrast = 0, currentContrast = 0, maxContrast = 0;

                    bool brightnessAvailable = GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, ref minBrightness, ref currentBrightness, ref maxBrightness);
                    bool contrastAvailable = GetMonitorContrast(physicalMonitor.hPhysicalMonitor, ref minContrast, ref currentContrast, ref maxContrast);

                    var info = new MonitorInfo
                    {
                        Handle = physicalMonitor.hPhysicalMonitor,
                        MinBrightness = minBrightness,
                        CurrentBrightness = currentBrightness,
                        MaxBrightness = maxBrightness,
                        MinContrast = minContrast,
                        CurrentContrast = currentContrast,
                        MaxContrast = maxContrast,
                        IsBrightnessAvailable = brightnessAvailable,
                        IsContrastAvailable = contrastAvailable
                    };

                    monitors.Add(info);
                }

                return true;
            }, nint.Zero);

            Monitors = monitors;
        }

        public void Dispose()
        {
            DisposeMonitors(Monitors);
            GC.SuppressFinalize(this);
        }

        private static void DisposeMonitors(IEnumerable<MonitorInfo> monitors)
        {
            if (monitors?.Any() == true)
            {
                PHYSICAL_MONITOR[] monitorArray = monitors.Select(m => new PHYSICAL_MONITOR { hPhysicalMonitor = m.Handle }).ToArray();
                DestroyPhysicalMonitors((uint)monitorArray.Length, monitorArray);
            }
        }

        #region Classes
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public nint hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public class MonitorInfo
        {
            public uint MinBrightness { get; set; }
            public uint MaxBrightness { get; set; }
            public uint CurrentBrightness { get; set; }
            public uint MinContrast { get; set; }
            public uint MaxContrast { get; set; }
            public uint CurrentContrast { get; set; }
            public nint Handle { get; set; }
            public bool IsBrightnessAvailable { get; set; }
            public bool IsContrastAvailable { get; set; }
        }
        #endregion
}