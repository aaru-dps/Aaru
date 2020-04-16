using System.Reactive;
using System.Threading;
using Aaru.Core;
using Aaru.Core.Devices.Scanning;
using Aaru.Core.Media.Info;
using Aaru.Devices;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;

namespace Aaru.Gui.ViewModels
{
    public class MediaScanViewModel : ViewModelBase
    {
        readonly Window _view;
        string          _a;
        string          _avgSpeed;
        string          _b;
        ulong           _blocksToRead;
        string          _c;
        bool            _closeVisible;
        string          _d;
        string          _devicePath;
        string          _e;
        string          _f;
        ScanResults     _localResults;
        string          _maxSpeed;
        string          _minSpeed;
        bool            _progress1Visible;
        string          _progress2Indeterminate;
        string          _progress2MaxValue;
        string          _progress2Text;
        string          _progress2Value;
        string          _progress2Visible;
        bool            _progressIndeterminate;
        double          _progressMaxValue;
        string          _progressText;
        double          _progressValue;
        bool            _progressVisible;
        bool            _resultsVisible;
        MediaScan       _scanner;
        bool            _startVisible;
        string          _stopEnabled;
        bool            _stopVisible;
        string          _totalTime;
        string          _unreadableSectors;
        /*
        static readonly Color LightGreen = Color.FromRgb(0x00FF00);
        static readonly Color Green      = Color.FromRgb(0x006400);
        static readonly Color DarkGreen  = Color.FromRgb(0x003200);
        static readonly Color Yellow     = Color.FromRgb(0xFFA500);
        static readonly Color Orange     = Color.FromRgb(0xFF4500);
        static readonly Color Red        = Color.FromRgb(0x800000);
        static          Color LightRed   = Color.FromRgb(0xFF0000);
        */

        public MediaScanViewModel(string devicePath, DeviceInfo deviceInfo, Window view, ScsiInfo scsiInfo = null)
        {
            _devicePath  = devicePath;
            _view        = view;
            StopVisible  = false;
            StartCommand = ReactiveCommand.Create(ExecuteStartCommand);
            CloseCommand = ReactiveCommand.Create(ExecuteCloseCommand);
            StopCommand  = ReactiveCommand.Create(ExecuteStopCommand);
            StartVisible = true;
            CloseVisible = true;

            /*
            lineChart.AbsoluteMargins = true;
            lineChart.MarginX         = 5;
            lineChart.MarginY         = 5;
            lineChart.DrawAxes        = true;
            lineChart.AxesColor       = Colors.Black;
            lineChart.ColorX          = Colors.Gray;
            lineChart.ColorY          = Colors.Gray;
            lineChart.BackgroundColor = Color.FromRgb(0x2974c1);
            lineChart.LineColor       = Colors.Yellow;
            */
        }

        public string A
        {
            get => _a;
            set => this.RaiseAndSetIfChanged(ref _a, value);
        }

        public string B
        {
            get => _b;
            set => this.RaiseAndSetIfChanged(ref _b, value);
        }

        public string C
        {
            get => _c;
            set => this.RaiseAndSetIfChanged(ref _c, value);
        }

        public string D
        {
            get => _d;
            set => this.RaiseAndSetIfChanged(ref _d, value);
        }

        public string E
        {
            get => _e;
            set => this.RaiseAndSetIfChanged(ref _e, value);
        }

        public string F
        {
            get => _f;
            set => this.RaiseAndSetIfChanged(ref _f, value);
        }

        public string UnreadableSectors
        {
            get => _unreadableSectors;
            set => this.RaiseAndSetIfChanged(ref _unreadableSectors, value);
        }

        public string TotalTime
        {
            get => _totalTime;
            set => this.RaiseAndSetIfChanged(ref _totalTime, value);
        }

        public string AvgSpeed
        {
            get => _avgSpeed;
            set => this.RaiseAndSetIfChanged(ref _avgSpeed, value);
        }

        public string MaxSpeed
        {
            get => _maxSpeed;
            set => this.RaiseAndSetIfChanged(ref _maxSpeed, value);
        }

        public string MinSpeed
        {
            get => _minSpeed;
            set => this.RaiseAndSetIfChanged(ref _minSpeed, value);
        }

        public bool ProgressVisible
        {
            get => _progressVisible;
            set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
        }

        public bool Progress1Visible
        {
            get => _progress1Visible;
            set => this.RaiseAndSetIfChanged(ref _progress1Visible, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        public double ProgressMaxValue
        {
            get => _progressMaxValue;
            set => this.RaiseAndSetIfChanged(ref _progressMaxValue, value);
        }

        public bool ProgressIndeterminate
        {
            get => _progressIndeterminate;
            set => this.RaiseAndSetIfChanged(ref _progressIndeterminate, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        public bool StartVisible
        {
            get => _startVisible;
            set => this.RaiseAndSetIfChanged(ref _startVisible, value);
        }

        public bool CloseVisible
        {
            get => _closeVisible;
            set => this.RaiseAndSetIfChanged(ref _closeVisible, value);
        }

        public bool StopVisible
        {
            get => _stopVisible;
            set => this.RaiseAndSetIfChanged(ref _stopVisible, value);
        }

        public string StopEnabled
        {
            get => _stopEnabled;
            set => this.RaiseAndSetIfChanged(ref _stopEnabled, value);
        }

        public bool ResultsVisible
        {
            get => _resultsVisible;
            set => this.RaiseAndSetIfChanged(ref _resultsVisible, value);
        }

        public string Title { get; }

        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand  { get; }

        void ExecuteCloseCommand() => _view.Close();

        internal void ExecuteStopCommand() => _scanner?.Abort();

        void ExecuteStartCommand()
        {
            StopVisible     = true;
            StartVisible    = false;
            CloseVisible    = false;
            ProgressVisible = true;
            ResultsVisible  = true;
            new Thread(DoWork).Start();
        }

        // TODO: Allow to save MHDD and ImgBurn log files
        async void DoWork()
        {
            if(_devicePath.Length == 2   &&
               _devicePath[1]     == ':' &&
               _devicePath[0]     != '/' &&
               char.IsLetter(_devicePath[0]))
                _devicePath = "\\\\.\\" + char.ToUpper(_devicePath[0]) + ':';

            var dev = new Device(_devicePath);

            if(dev.IsRemote)
                Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                     dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

            if(dev.Error)
            {
                await MessageBoxManager.
                      GetMessageBoxStandardWindow("Error", $"Error {dev.LastError} opening device.", ButtonEnum.Ok,
                                                  Icon.Error).ShowDialog(_view);

                StopVisible     = false;
                StartVisible    = true;
                CloseVisible    = true;
                ProgressVisible = false;

                return;
            }

            Statistics.AddDevice(dev);

            _localResults                 =  new ScanResults();
            _scanner                      =  new MediaScan(null, null, _devicePath, dev);
            _scanner.ScanTime             += OnScanTime;
            _scanner.ScanUnreadable       += OnScanUnreadable;
            _scanner.UpdateStatus         += UpdateStatus;
            _scanner.StoppingErrorMessage += StoppingErrorMessage;
            _scanner.PulseProgress        += PulseProgress;
            _scanner.InitProgress         += InitProgress;
            _scanner.UpdateProgress       += UpdateProgress;
            _scanner.EndProgress          += EndProgress;
            _scanner.InitBlockMap         += InitBlockMap;
            _scanner.ScanSpeed            += ScanSpeed;

            ScanResults results = _scanner.Scan();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TotalTime =
                    $"Took a total of {results.TotalTime} seconds ({results.ProcessingTime} processing commands).";

                AvgSpeed          = $"Average speed: {results.AvgSpeed:F3} MiB/sec.";
                MaxSpeed          = $"Fastest speed burst: {results.MaxSpeed:F3} MiB/sec.";
                MinSpeed          = $"Slowest speed burst: {results.MinSpeed:F3} MiB/sec.";
                A                 = $"{results.A} sectors took less than 3 ms.";
                B                 = $"{results.B} sectors took less than 10 ms but more than 3 ms.";
                C                 = $"{results.C} sectors took less than 50 ms but more than 10 ms.";
                D                 = $"{results.D} sectors took less than 150 ms but more than 50 ms.";
                E                 = $"{results.E} sectors took less than 500 ms but more than 150 ms.";
                F                 = $"{results.F} sectors took more than 500 ms.";
                UnreadableSectors = $"{results.UnreadableSectors.Count} sectors could not be read.";
            });

            // TODO: Show list of unreadable sectors
            /*
            if(results.UnreadableSectors.Count > 0)
                foreach(ulong bad in results.UnreadableSectors)
                    string.Format("Sector {0} could not be read", bad);
*/

            // TODO: Show results
            /*
            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if(results.SeekTotal != 0 || results.SeekMin != double.MaxValue || results.SeekMax != double.MinValue)
                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                string.Format("Testing {0} seeks, longest seek took {1:F3} ms, fastest one took {2:F3} ms. ({3:F3} ms average)",
                                     results.SeekTimes, results.SeekMax, results.SeekMin, results.SeekTotal / 1000);
                                     */

            Statistics.AddCommand("media-scan");

            dev.Close();
            WorkFinished();
        }

        async void ScanSpeed(ulong sector, double currentspeed) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            /* TODO: Chart
            if(currentspeed > lineChart.MaxY)
                lineChart.MaxY = (float)(currentspeed + (currentspeed / 10));

            lineChart.Values.Add(new PointF(sector, (float)currentspeed));
            */
        });

        async void InitBlockMap(ulong blocks, ulong blocksize, ulong blockstoread, ushort currentProfile) =>
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                /* TODO: BlockMap
                blockMap.Sectors       = blocks;
                blockMap.SectorsToRead = (uint)blockstoread;
                blocksToRead           = blockstoread;
                lineChart.MinX         = 0;
                lineChart.MinY         = 0;

                switch(currentProfile)
                {
                    case 0x0005: // CD and DDCD
                    case 0x0008:
                    case 0x0009:
                    case 0x000A:
                    case 0x0020:
                    case 0x0021:
                    case 0x0022:
                        if(blocks <= 360000)
                            lineChart.MaxX = 360000;
                        else if(blocks <= 405000)
                            lineChart.MaxX = 405000;
                        else if(blocks <= 445500)
                            lineChart.MaxX = 445500;
                        else
                            lineChart.MaxX = blocks;

                        lineChart.StepsX = lineChart.MaxX   / 10f;
                        lineChart.StepsY = 150              * 4;
                        lineChart.MaxY   = lineChart.StepsY * 12.5f;

                        break;
                    case 0x0010: // DVD SL
                    case 0x0011:
                    case 0x0012:
                    case 0x0013:
                    case 0x0014:
                    case 0x0018:
                    case 0x001A:
                    case 0x001B:
                        lineChart.MaxX   = 2298496;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 1352.5f;
                        lineChart.MaxY   = lineChart.StepsY * 26;

                        break;
                    case 0x0015: // DVD DL
                    case 0x0016:
                    case 0x0017:
                    case 0x002A:
                    case 0x002B:
                        lineChart.MaxX   = 4173824;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 1352.5f;
                        lineChart.MaxY   = lineChart.StepsY * 26;

                        break;
                    case 0x0041:
                    case 0x0042:
                    case 0x0043:
                    case 0x0040: // BD
                        if(blocks <= 12219392)
                            lineChart.MaxX = 12219392;
                        else if(blocks <= 24438784)
                            lineChart.MaxX = 24438784;
                        else if(blocks <= 48878592)
                            lineChart.MaxX = 48878592;
                        else if(blocks <= 62500864)
                            lineChart.MaxX = 62500864;
                        else
                            lineChart.MaxX = blocks;

                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 4394.5f;
                        lineChart.MaxY   = lineChart.StepsY * 18;

                        break;
                    case 0x0050: // HD DVD
                    case 0x0051:
                    case 0x0052:
                    case 0x0053:
                    case 0x0058:
                    case 0x005A:
                        if(blocks <= 7361599)
                            lineChart.MaxX = 7361599;
                        else if(blocks <= 16305407)
                            lineChart.MaxX = 16305407;
                        else
                            lineChart.MaxX = blocks;

                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 4394.5f;
                        lineChart.MaxY   = lineChart.StepsY * 8;

                        break;
                    default:
                        lineChart.MaxX   = blocks;
                        lineChart.StepsX = lineChart.MaxX / 10f;
                        lineChart.StepsY = 625f;
                        lineChart.MaxY   = lineChart.StepsY;

                        break;
                }
                */
            });

        async void WorkFinished() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            StopVisible     = false;
            StartVisible    = true;
            CloseVisible    = true;
            ProgressVisible = false;
        });

        async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress1Visible = false;
        });

        async void UpdateProgress(string text, long current, long maximum) =>
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressText          = text;
                ProgressIndeterminate = false;

                ProgressMaxValue = maximum;
                ProgressValue    = current;
            });

        async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress1Visible = true;
        });

        async void PulseProgress(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText          = text;
            ProgressIndeterminate = true;
        });

        async void StoppingErrorMessage(string text) => await Dispatcher.UIThread.InvokeAsync(action: async () =>
        {
            ProgressText = text;

            await MessageBoxManager.GetMessageBoxStandardWindow("Error", $"{text}", ButtonEnum.Ok, Icon.Error).
                                    ShowDialog(_view);

            WorkFinished();
        });

        async void UpdateStatus(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressText = text;
        });

        async void OnScanUnreadable(ulong sector) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _localResults.Errored += _blocksToRead;
            UnreadableSectors     =  $"{_localResults.Errored} sectors could not be read.";
            /* TODO: Blockmap
            blockMap.ColoredSectors.Add(new ColoredBlock(sector, LightGreen));
            */
        });

        async void OnScanTime(ulong sector, double duration) => await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if(duration < 3)
            {
                _localResults.A += _blocksToRead;
                /* TODO: Blockmap
                    blockMap.ColoredSectors.Add(new ColoredBlock(sector, LightGreen));
                */
            }
            else if(duration >= 3 &&
                    duration < 10)
            {
                _localResults.B += _blocksToRead;
                /* TODO: Blockmap
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, Green));
                */
            }
            else if(duration >= 10 &&
                    duration < 50)
            {
                _localResults.C += _blocksToRead;
                /* TODO: Blockmap
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, DarkGreen));
                */
            }
            else if(duration >= 50 &&
                    duration < 150)
            {
                _localResults.D += _blocksToRead;
                /* TODO: Blockmap
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, Yellow));
                */
            }
            else if(duration >= 150 &&
                    duration < 500)
            {
                _localResults.E += _blocksToRead;
                /* TODO: Blockmap
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, Orange));
                */
            }
            else if(duration >= 500)
            {
                _localResults.F += _blocksToRead;
                /* TODO: Blockmap
                blockMap.ColoredSectors.Add(new ColoredBlock(sector, Red));
                */
            }

            A = $"{_localResults.A} sectors took less than 3 ms.";
            B = $"{_localResults.B} sectors took less than 10 ms but more than 3 ms.";
            C = $"{_localResults.C} sectors took less than 50 ms but more than 10 ms.";
            D = $"{_localResults.D} sectors took less than 150 ms but more than 50 ms.";
            E = $"{_localResults.E} sectors took less than 500 ms but more than 150 ms.";
            F = $"{_localResults.F} sectors took more than 500 ms.";
        });
    }
}