using JTalkDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms.Design;
using System.Configuration;

namespace FormWithButton
{
    public enum ScriptPadMode
    {
        Sequential,
        Functional,
    }

    public class Settings : ApplicationSettingsBase
    {
        public static readonly string DefaultVoiceName = "tohoku-f01-neutral";

        public static readonly AudioDeviceInfo DefaultAudioDeviceInfo = new AudioDeviceInfo();

        public static VoiceFileInfo MatchVoice<C>(
            String voiceName,
            VoiceFileInfo defaultVoice,
            C voices
        ) where C : IEnumerable<VoiceFileInfo>
        {
            return voices
                .Where(v => v.Name == voiceName)
                .DefaultIfEmpty(defaultVoice)
                .First();
        }

        public static AudioDeviceInfo MatchAudioDevice<C>(
            AudioDeviceInfo device,
            C devices
        ) where C : IEnumerable<AudioDeviceInfo>
        {
            AudioDeviceInfo imDevice = null;
            AudioDeviceInfo nmDevice = null;
            foreach (var elem in devices)
            {
                bool im = elem.Index == device.Index;
                bool nm = elem.Name == device.Name;
                if (im && nm)
                {
                    return elem;
                }
                else if (nm)
                {
                    if (nmDevice == null)
                    {
                        nmDevice = elem;
                    }
                }
                else if (im)
                {
                    if (imDevice == null)
                    {
                        imDevice = elem;
                    }
                }
            }

            return nmDevice != null ?
                    nmDevice :
                    imDevice != null ?
                    imDevice : DefaultAudioDeviceInfo;
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public String VoiceName
        {
            get { return (string)this["VoiceName"]; }
            set { this["VoiceName"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("-1")]
        public int AudioOutputDeviceIndex
        {
            get { return (int)this["AudioOutputDeviceIndex"]; }
            set { this["AudioOutputDeviceIndex"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public String AudioOutputDeviceName
        {
            get { return (string)this["AudioOutputDeviceName"]; }
            set { this["AudioOutputDeviceName"] = value; }
        }

        public AudioDeviceInfo AudioOutputDevice
        {
            get { return new AudioDeviceInfo(this.AudioOutputDeviceIndex, this.AudioOutputDeviceName); }
            set
            {
                this.AudioOutputDeviceIndex = value.Index;
                this.AudioOutputDeviceName = value.Name;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0.55")]
        public double Alpha
        {
            get { return (double)this["Alpha"]; }
            set { this["Alpha"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0.0")]
        public double Beta
        {
            get { return (double)this["Beta"]; }
            set { this["Beta"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("1.0")]
        public double Speed
        {
            get { return (double)this["Speed"]; }
            set { this["Speed"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0.0")]
        public double AdditionalHalfTone
        {
            get { return (double)this["AdditionalHalfTone"]; }
            set { this["AdditionalHalfTone"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0.0")]
        public double MsdThreshold
        {
            get { return (double)this["MsdThreshold"]; }
            set { this["MsdThreshold"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("1.0")]
        public double GvWeightForSpectrum
        {
            get { return (double)this["GvWeightForSpectrum"]; }
            set { this["GvWeightForSpectrum"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("1.0")]
        public double GvWeightForLogF0
        {
            get { return (double)this["GvWeightForLogF0"]; }
            set { this["GvWeightForLogF0"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("0.0")]
        public double Volume
        {
            get { return (double)this["Volume"]; }
            set { this["Volume"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("Sequential")]
        public ScriptPadMode ScriptPadMode
        {
            get { return (ScriptPadMode)this["ScriptPadMode"]; }
            set { this["ScriptPadMode"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("-1")]
        public double MainWindowLeft
        {
            get { return (double)this["MainWindowLeft"]; }
            set { this["MainWindowLeft"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("-1")]
        public double MainWindowTop
        {
            get { return (double)this["MainWindowTop"]; }
            set { this["MainWindowTop"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("-1")]
        public double MainWindowWidth
        {
            get { return (double)this["MainWindowWidth"]; }
            set { this["MainWindowWidth"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("-1")]
        public double MainWindowHeight
        {
            get { return (double)this["MainWindowHeight"]; }
            set { this["MainWindowHeight"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("Normal")]
        public FormWindowState MainWindowState
        {
            get { return (FormWindowState)this["MainWindowState"]; }
            set { this["MainWindowState"] = value; }
        }
    }

    public class Form1 : Form
    {
        private Settings settings = new Settings();
        private JTalkTTS tts = new JTalkTTS { VoiceName = Settings.DefaultVoiceName };
        private ScriptPadMode scriptPadMode;
        private ComboBox comboBox1;
        private ComboBox audioDeviceComboBox;
        private RichTextBox richTextBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private ToolTip settingsToolTip;
        private CancellationTokenSource cancelToken;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;

        private static bool DirectoryExists(string path)
        {
            try
            {
                return System.IO.Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        private static T ElementAtOrElse<T, C>(
            int indexOfElement,
            T defaultValue,
            C collection
        ) where C : IEnumerable<T>
        {
            return indexOfElement >= 0 ?
                collection.Skip(indexOfElement).DefaultIfEmpty(defaultValue).First() :
                defaultValue;
        }

        private async void SayText(
            String text,
            Tuple<int, int> range,
            CancellationTokenSource token,
            Action<Tuple<int, int>> onNext,
            Action onComplete
        )
        {
            var textLength = text.Length;
            var start = range.Item1;
            var end = range.Item2;
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("range", "Start < 0");
            }

            if (end < start)
            {
                throw new ArgumentOutOfRangeException("range", "End < Start");
            }

            if (end > textLength)
            {
                throw new ArgumentOutOfRangeException("range", "End > text.Length");
            }

            await Task.Run(() =>
            {
                var pos = start;
                var data = "";
                var length = 0;
                var lastLine = false;
                try
                {
                    while (true)
                    {
                        var substr = text.Substring(pos, end - pos);
                        var m = Regex.Match(substr, @"^([^。\n]*?)(。\n|。|\n)");
                        if (m.Success)
                        {
                            length = m.Length;
                            data = m.Value;
                        }
                        else
                        {
                            lastLine = true;
                            length = substr.Length;
                            data = substr;
                        }

                        data = data.Trim(' ', '\t', '　', '\r', '\n');
                        if (data != "")
                        {
                            Invoke(new Action(() =>
                            {
                                onNext(Tuple.Create(pos, pos + length));
                                this.tts.SpeakAsync(data);
                            }));
                            while (this.tts.IsSpeaking)
                            {
                                token.Token.ThrowIfCancellationRequested();
                            }
                        }

                        if (lastLine)
                        {
                            break;
                        }

                        if (pos + length >= end)
                        {
                            break;
                        }
                        pos += length;
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // do nothing
                }

                Invoke(new Action(() => {
                    onNext(Tuple.Create(pos + length, pos + length));
                    onComplete();
                }));
            });
        }

        private void ExecSayCommandSequential()
        {
            var text = this.richTextBox1.Text;
            var textLength = text.Length;
            if (textLength <= 0)
            {
                return;
            }

            Tuple<int, int> range;
            var start = this.richTextBox1.SelectionStart;
            var len = this.richTextBox1.SelectionLength;
            if (len > 0)
            {
                range = Tuple.Create(start, start + len);
            }
            else if (start < textLength)
            {
                range = Tuple.Create(start, textLength);
            }
            else
            {
                range = Tuple.Create(0, textLength);
            }

            this.button1.Enabled = false;
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Focus();

            this.cancelToken = new CancellationTokenSource();

            SayText(
                text,
                range,
                this.cancelToken,
                (nextRange) =>
                {
                    this.richTextBox1.Select(nextRange.Item1, nextRange.Item2 - nextRange.Item1);
                },
                () =>
                {
                    this.richTextBox1.ReadOnly = false;
                    this.button1.Enabled = true;
                    this.richTextBox1.Focus();
                }
            );
        }

        private void ExecSayCommandFunctional()
        {
            var text = this.richTextBox1.Text;
            var textLength = text.Length;
            if (textLength <= 0)
            {
                return;
            }

            Tuple<int, int> range = Tuple.Create(0, textLength);

            this.button1.Enabled = false;
            this.richTextBox1.Focus();

            this.cancelToken = new CancellationTokenSource();

            SayText(
                text,
                range,
                this.cancelToken,
                (nextRange) =>
                {
                    // do nothing
                },
                () =>
                {
                    if (text == this.richTextBox1.Text)
                    {
                        this.richTextBox1.Select(0, this.richTextBox1.TextLength);
                    }
                    this.button1.Enabled = true;
                    this.richTextBox1.Focus();
                }
            );
        }

        private void ExecSayCommand()
        {
            switch (this.scriptPadMode)
            {
                case ScriptPadMode.Functional:
                    ExecSayCommandFunctional();
                    break;
                default:
                    ExecSayCommandSequential();
                    break;
            }
        }

        private void ExecCancelCommand()
        {
            if (this.cancelToken != null)
            {
                this.cancelToken.Cancel();
            }

            if (this.tts.IsSpeaking)
            {
                this.tts.Stop();
            }
        }

        private string ReadJapaneseText(string path)
        {
            var encode = "Shift_JIS";
            var sr = new StreamReader(path, Encoding.GetEncoding(encode));
            var text = sr.ReadToEnd();
            sr.Close();
            if (text.Length > 10000)
            {
                throw new Exception("ファイルが大きすぎます。");
            }

            return text;
        }

        private bool LoadTalkSettings()
        {
            try
            {
                var alpha = this.settings.Alpha;
                if (!Double.IsNaN(alpha)) { this.tts.Alpha = alpha; }

                var beta = this.settings.Beta;
                if (!Double.IsNaN(beta)) { this.tts.Beta = beta; }

                var speed = this.settings.Speed;
                if (!Double.IsNaN(speed)) { this.tts.Speed = speed; }

                var additionalHalfTone = this.settings.AdditionalHalfTone;
                if (!Double.IsNaN(additionalHalfTone)) { this.tts.AdditionalHalfTone = additionalHalfTone; }

                var msdThreshold = this.settings.MsdThreshold;
                if (!Double.IsNaN(msdThreshold)) { this.tts.MSDThreshold = msdThreshold; }

                var gvWeightForSpectrum = this.settings.GvWeightForSpectrum;
                if (!Double.IsNaN(gvWeightForSpectrum)) { this.tts.GVWeightForSpectrum = gvWeightForSpectrum; }

                var gvWeightForLogF0 = this.settings.GvWeightForLogF0;
                if (!Double.IsNaN(gvWeightForLogF0)) { this.tts.GVWeightForLogF0 = gvWeightForLogF0; }

                var volume = this.settings.Volume;
                if (!Double.IsNaN(volume)) { this.tts.Volume = volume; }

                return true;
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            return false;
        }

        private bool LoadSettings()
        {
            try
            {
                this.comboBox1.SelectedItem = Settings.MatchVoice(this.settings.VoiceName, this.tts.Voice, this.tts.Voices).Name;
                this.audioDeviceComboBox.SelectedItem = Settings.MatchAudioDevice(this.settings.AudioOutputDevice, this.tts.AudioOutputDevices).Name;

                LoadTalkSettings();

                this.scriptPadMode = this.settings.ScriptPadMode;

                var windowLeft = this.settings.MainWindowLeft;
                var windowTop = this.settings.MainWindowTop;
                var windowWidth = this.settings.MainWindowWidth;
                var windowHeight = this.settings.MainWindowHeight;

                if (windowLeft >= 0 && windowTop >= 0)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Left = (int) windowLeft;
                    this.Top = (int) windowTop;
                }

                if (windowWidth >= 0) { this.Width = (int) windowWidth; }
                if (windowHeight >= 0) { this.Height = (int) windowHeight; }

                this.WindowState = this.settings.MainWindowState;

                return true;
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            return false;
        }

        private bool SaveSettings()
        {
            try
            {
                this.settings.VoiceName = ElementAtOrElse(
                    this.comboBox1.SelectedIndex,
                    this.tts.Voice,
                    this.tts.Voices
                ).Name;

                this.settings.AudioOutputDevice = ElementAtOrElse(
                    this.audioDeviceComboBox.SelectedIndex,
                    Settings.DefaultAudioDeviceInfo,
                    this.tts.AudioOutputDevices
                );

                this.settings.Alpha = this.tts.Alpha;
                this.settings.Beta = this.tts.Beta;
                this.settings.Speed = this.tts.Speed;
                this.settings.AdditionalHalfTone = this.tts.AdditionalHalfTone;
                this.settings.MsdThreshold = this.tts.MSDThreshold;
                this.settings.GvWeightForSpectrum = this.tts.GVWeightForSpectrum;
                this.settings.GvWeightForLogF0 = this.tts.GVWeightForLogF0;
                this.settings.Volume = this.tts.Volume;

                this.settings.ScriptPadMode = this.scriptPadMode;

                if (WindowState == FormWindowState.Normal)
                {
                    this.settings.MainWindowLeft = this.Left;
                    this.settings.MainWindowTop = this.Top;
                    this.settings.MainWindowWidth = this.Width;
                    this.settings.MainWindowHeight = this.Height;
                }

                this.settings.MainWindowState = this.WindowState;

                this.settings.Save();
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            return false;
        }

        public Form1()
        {
            int tabIndex = 0;

            // 音響モデルフォルダが変更されたときの処理の登録
            this.tts.VoiceListChanged += (sender, e) =>
            {
                this.comboBox1.Items.Clear();
                this.comboBox1.Items.AddRange(this.tts.Voices.Select(v => v.Name).ToArray());
                this.comboBox1.SelectedItem = this.tts.Voice.Name;
            };

            this.Closing += (sender, e) =>
            {
                if (!e.Cancel)
                {
                    SaveSettings();
                }
            };
            this.Size = new Size(360, 240);
            this.MinimumSize = this.Size;
            this.Text = "発声テスト";

            this.comboBox1 = new ComboBox();
            this.comboBox1.Dock = DockStyle.Fill;
            this.comboBox1.TabIndex = tabIndex ++;
            this.comboBox1.SelectedIndexChanged += (sender, e) => {
                this.tts.Voice = ElementAtOrElse(
                    this.comboBox1.SelectedIndex,
                    this.tts.Voice,
                    this.tts.Voices
                );

                LoadTalkSettings();
            };
            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox1.Items.AddRange(this.tts.Voices.Select(v => v.Name).ToArray());

            this.audioDeviceComboBox = new ComboBox();
            this.audioDeviceComboBox.Dock = DockStyle.Fill;
            this.audioDeviceComboBox.TabIndex = tabIndex ++;
            this.audioDeviceComboBox.SelectedIndexChanged += (sender, e) =>
            {
                this.tts.AudioOutputDevice = ElementAtOrElse(
                    this.audioDeviceComboBox.SelectedIndex,
                    Settings.DefaultAudioDeviceInfo,
                    this.tts.AudioOutputDevices
                ).Index;
            };
            this.audioDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.audioDeviceComboBox.Items.AddRange(this.tts.AudioOutputDevices.Select(v => v.Name).ToArray());

            this.richTextBox1 = new RichTextBox();
            this.richTextBox1.Dock = DockStyle.Fill;
            this.richTextBox1.Text = "何か入力してください";
            this.richTextBox1.Select(0, this.richTextBox1.TextLength);
            this.richTextBox1.TabIndex = tabIndex ++;
            this.richTextBox1.AllowDrop = true;
            this.richTextBox1.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Shift)
                {
                    if (this.tts.IsSpeaking)
                    {
                        this.ExecCancelCommand();
                    }
                    else
                    {
                        this.ExecSayCommand();
                    }
                    e.Handled = true;
                }
            };
            this.richTextBox1.DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var file in files)
                    {
                        if (!System.IO.File.Exists(file))
                        {
                            return;
                        }
                    }
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            };
            this.richTextBox1.DragDrop += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var keep = this.richTextBox1.Text;
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    this.richTextBox1.Text = "";
                    foreach (var file in files)
                    {
                        var text = this.ReadJapaneseText(file);
                        if (text != "")
                        {
                            this.richTextBox1.Text += text;
                        }
                        else
                        {
                            this.richTextBox1.Text = keep;
                            return;
                        }
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            };

            this.button1 = new Button();
            this.button1.Dock = DockStyle.Fill;
            this.button1.Text = "発声";
            this.button1.TabIndex = tabIndex ++;
            this.button1.Click += (sender, e) =>
            {
                this.ExecSayCommand();
            };

            this.button2 = new Button();
            this.button2.Dock = DockStyle.Fill;
            this.button2.Text = "停止";
            this.button2.TabIndex = tabIndex ++;
            this.button2.Click += (sender, e) =>
            {
                this.ExecCancelCommand();
            };

            this.button3 = new Button();
            this.button3.Dock = DockStyle.Fill;
            this.button3.Text = "音声フォルダ変更";
            this.button3.TabIndex = tabIndex ++;
            this.button3.Click += (sender, e) =>
            {
                var dialog = new FolderBrowserDialog();
                dialog.Description = "音響モデルフォルダ選択";
                dialog.SelectedPath = this.tts.VoiceDir;
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.tts.VoiceDir = dialog.SelectedPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            };

            string settingsDir = "";
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.PerUserRoamingAndLocal
                );
                settingsDir = System.IO.Path.GetDirectoryName(config.FilePath) ?? "";
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }

            this.button4 = new Button();
            this.button4.Dock = DockStyle.Fill;
            this.button4.Text = "設定...";
            this.button4.TabIndex = tabIndex ++;
            this.button4.Enabled = DirectoryExists(settingsDir);
            this.button4.Click += (sender, e) =>
            {
                try
                {
                    if (DirectoryExists(settingsDir))
                    {
                        System.Diagnostics.Process.Start(settingsDir);
                    }
                }
                catch
                {
                    // do nothing
                }

                Help.ShowPopup(this, settingsDir, Control.MousePosition);
            };

            this.settingsToolTip = new ToolTip();
            this.settingsToolTip.SetToolTip(button4, settingsDir);

            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.tableLayoutPanel2 = new TableLayoutPanel();

            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this.tableLayoutPanel2.Controls.Add(this.button1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.button3, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.button4, 3, 0);
            this.tableLayoutPanel2.Dock = DockStyle.Fill;
            this.tableLayoutPanel2.Location = new Point(0, 121);
            this.tableLayoutPanel2.Margin = new Padding(0);
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new Size(276, 40);
            this.tableLayoutPanel2.TabIndex = 1;

            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.comboBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.audioDeviceComboBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Location = new Point(0, 0);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new Size(276, 161);
            this.tableLayoutPanel1.TabIndex = 0;

            this.Controls.Add(this.tableLayoutPanel1);
            this.ActiveControl = this.richTextBox1;

            LoadSettings();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }
    }
}
