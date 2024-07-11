using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using NAudio.Wave;

namespace Bejeweled
{
    public partial class Bejeweled : Form
    {
        public Bejeweled()
        {
            InitializeComponent();

            //InitializeGrid();

            // 创建FlowLayoutPanel放置button -<
            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(controlPanel);

            InitializeLabels(controlPanel);
            InitializeButtons(controlPanel);
            InitializeTimer();

            //InitializeGrid(controlPanel);
        }

        private Label labelScore;
        private Label labelCountdown;
        private Button btnStart;
        private Button btnRestart;
        private Timer timerCountdown;

        private Panel gridPanel; // 8x8网格的Panel

        private int score = 0;
        private bool isGameStarted = false;
        private bool isGameMusicStarted = false;

        private IWavePlayer waveOut;
        private Mp3FileReader mp3FileReader;

        private void PlayMusic()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string musicPath = Path.Combine(currentDirectory, "..\\..\\Properties\\music\\blackgroundmusic.mp3");

            if (!File.Exists(musicPath))
            {
                MessageBox.Show("The MP3 file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mp3FileReader = new Mp3FileReader(musicPath);

            if (waveOut == null)
            {
                waveOut = new WaveOutEvent();
            }

            if (!isGameMusicStarted) 
            {
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    // 重新播放
                    mp3FileReader?.Dispose();
                    mp3FileReader = new Mp3FileReader(musicPath);
                    waveOut.Init(mp3FileReader);
                    waveOut.Play();
                    //isGameMusicStarted = true;
                };
            }
            
            waveOut.Init(mp3FileReader);
            waveOut.Play();
            isGameMusicStarted = true;
        }

        private void StopMusic()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                mp3FileReader?.Dispose();
                isGameMusicStarted = false;
            }
        }

        private void UpdateScoreDisplay()
        {
            labelScore.Text = $"Score: {score}";
        }

        private void InitializeLabels(Panel panel)
        {
            // 分数标签
            labelScore = new Label
            {
                Text = "Score: 0",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            panel.Controls.Add(labelScore);

            // 倒计时标签
            labelCountdown = new Label
            {
                Text = "Time: 60",
                AutoSize = true,
                Location = new Point(10, 40)
            };
            panel.Controls.Add(labelCountdown);
        }

        private void InitializeButtons(Panel panel)
        {
            // 开始button
            btnStart = new Button
            {
                Text = "Start",
                Size = new Size(180, 50),
                Location = new Point(10, 70)
            };
            btnStart.Click += BtnStart_Click;
            panel.Controls.Add(btnStart);

            // 重新开始button
            btnRestart = new Button
            {
                Text = "Restart",
                Size = new Size(180, 50),
                Location = new Point(10, 130)
            };
            btnRestart.Click += BtnRestart_Click;
            panel.Controls.Add(btnRestart);
        }

        private void InitializeTimer()
        {
            timerCountdown = new Timer();
            timerCountdown.Interval = 1000;
            timerCountdown.Tick += TimerCountdown_Tick;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!isGameStarted)
            {
                score = 0;
                UpdateScoreDisplay();
                timerCountdown.Start();
                isGameStarted = true;
                InitializeGrid();
                if(!isGameMusicStarted)
                {
                    isGameMusicStarted = true;
                    PlayMusic();
                }
            }
            else
            {
                MessageBox.Show("Game has already started!");
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            isGameStarted = false;
            score = 0;
            labelCountdown.Text = "Time: 60";
            timerCountdown.Stop();
            UpdateScoreDisplay();
            ClearGrid();
            InitializeGrid();
            StopMusic();
        }

        private void TimerCountdown_Tick(object sender, EventArgs e)
        {
            // 每次计时器触发时，时间减 1
            int timeLeft = int.Parse(labelCountdown.Text.Split(':')[1].Trim()) - 1;

            // 更新时间标签
            labelCountdown.Text = $"Time: {timeLeft}";

            // 检查时间是否已经用完
            if (timeLeft <= 0)
            {
                timerCountdown.Stop();
                isGameStarted = false; // 游戏结束，重置状态
                var result = MessageBox.Show(
                    "Time's up! Final Score: " + score + ".\n" +
                    "Do you want to restart the game?",
                    "Game Over", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    BtnRestart_Click(sender, e);
                }
                else if (result == DialogResult.No)
                {
                    ClearGrid(); // 清除网格
                    Application.Exit();
                }
            }
        }

        private void InitializeGrid()
        {
            if (gridPanel != null)
            {
                this.Controls.Remove(gridPanel);
                gridPanel.Dispose();
            }

            int buttonSize = 50;
            int gridWidth = 8;
            int gridHeight = 8;
            int padding = 1;
            int numberOfImages = 6;

            int gridPanelWidth = (buttonSize + padding) * gridWidth - padding;
            int gridPanelHeight = gridPanelWidth;

            gridPanel = new Panel
            {
                Size = new Size(gridPanelWidth, gridPanelHeight),
                Location = new Point(200 + padding, (this.ClientSize.Height - gridPanelHeight) / 2),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(gridPanel);

            Random random = new Random();
            string lastImageName = null; // 用于记录上一个使用的图片名称
            string[] images = Enumerable.Range(1, numberOfImages)
                                          .Select(i => $"button_image{i}")
                                          .ToArray(); // 所有图片资源名称的数组

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    string resourceName;
                    if (random.NextDouble() < 0.05 && lastImageName != null) // 有5%的概率使用上一个图片
                    {
                        resourceName = lastImageName;
                    }
                    else // 否则随机选择一个新的图片
                    {
                        resourceName = images[random.Next(images.Length)];
                        lastImageName = resourceName; // 更新上一个使用的图片名称
                    }

                    Image image = Properties.Resources.ResourceManager.GetObject(resourceName) as Image;

                    Button button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(col * (buttonSize + padding), row * (buttonSize + padding)),
                        BackColor = Color.Transparent // 设置按钮背景色为透明
                    };

                    if (image != null)
                    {
                        button.BackgroundImageLayout = ImageLayout.Zoom; // 根据需要设置图片布局
                        button.BackgroundImage = image;
                    }
                    else
                    {
                        MessageBox.Show($"Resource named {resourceName} not found.", "Resource Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    gridPanel.Controls.Add(button);
                    button.Click += Button_Click;
                }
            }
        }

        private void ClearGrid()
        {
            if (gridPanel != null)
            {
                this.Controls.Remove(gridPanel);
                gridPanel.Dispose();
                gridPanel = null;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            // 按钮点击事件处理
            Button clickedButton = sender as Button;
            MessageBox.Show($"Button at position ({clickedButton.Location.X}, {clickedButton.Location.Y}) was clicked.");
        }

    }
}