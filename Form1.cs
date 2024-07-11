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
        private int buttonSize;
        private int gridHeight;
        private int gridWidth;
        string[,] grid = new string[8, 8];

        private int score = 0;
        private bool isGameStarted = false;
        private bool isGameMusicStarted = false;

        private Button firstClickedButton;
        private Button secondClickedButton;

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
            isGameMusicStarted = false;
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
                Text = "Time: 60000",
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

                CheckAndEliminateMatches();

                if (!isGameMusicStarted)
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
            labelCountdown.Text = "Time: 60000";
            timerCountdown.Stop();
            UpdateScoreDisplay();
            ClearGrid();
            //InitializeGrid();
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

            buttonSize = 50;
            gridWidth = 8;
            gridHeight = 8;
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

                    grid[row, col] = resourceName;

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

        private void CheckAndEliminateMatches()
        {
            bool anyMatchFound;
            do
            {
                anyMatchFound = false;
                int matchCount;

                // 检查行
                for (int row = 0; row < gridHeight; row++)
                {
                    for (int col = 0; col < gridWidth - 2; col++)
                    {
                        if (grid[row, col] != null && IsMatchInRow(row, col))
                        {
                            matchCount = 3;
                            while (col + matchCount < gridWidth 
                                && grid[row, col + matchCount] != null 
                                && grid[row, col] == grid[row, col + matchCount])
                            {
                                matchCount++;
                            }
                            anyMatchFound = true;
                            EliminateRowMatches(row, col, matchCount);
                            AddScore(matchCount);
                            break;
                        }
                    }
                }

                // 检查列
                for (int col = 0; col < gridWidth; col++)
                {
                    for (int row = 0; row < gridHeight - 2; row++)
                    {
                        if (grid[row, col] != null && IsMatchInColumn(col, row))
                        {
                            matchCount = 3;
                            while (row + matchCount < gridHeight
                                && grid[row + matchCount, col] != null
                                && grid[row, col] == grid[row + matchCount, col])
                            {
                                matchCount++;
                            }
                            anyMatchFound = true;
                            EliminateColumnMatches(col, row, matchCount);
                            AddScore(matchCount);
                            break;
                        }
                    }
                }

                // 更新界面显示
                if (anyMatchFound)
                {
                    UpdateGridDisplay();
                }

            } while (anyMatchFound);
        }

        private void AddScore(int matchCount)
        {
            int scoreToAdd = (int)Math.Pow(2, matchCount - 2);
            score += scoreToAdd;
            UpdateScoreDisplay();
        }

        private void UpdateGridDisplay()
        {
            var controlsToRemove = gridPanel.Controls.OfType<Button>().Where(button =>
            {
                int gridY = button.Location.Y / buttonSize;
                int gridX = button.Location.X / buttonSize;
                return grid[gridY, gridX] == null;
            }).ToList();

            int maxSize = 70;
            int interval = 10;
            int increaseStep = 2;

            foreach (var button in controlsToRemove)
            {
                // 创建动画 Timer
                Timer animationTimer = new Timer();
                animationTimer.Interval = interval;
                bool isIncreasing = true;

                animationTimer.Tick += (sender, e) =>
                {
                    if (isIncreasing)
                    {
                        // 放大动画
                        if (button.Width < maxSize)
                        {
                            int step = increaseStep;
                            button.Width += step;
                            button.Height += step;
                            button.Left -= (step / 2);
                            button.Top -= (step / 2);
                        }
                        else
                        {
                            isIncreasing = false;
                        }
                    }
                    else
                    {
                        // 缩小动画
                        int step = increaseStep;
                        button.Width -= step;
                        button.Height -= step;
                        button.Left += (step / 2);
                        button.Top += (step / 2);

                        if (button.Width <= 0 || button.Height <= 0)
                        {
                            animationTimer.Stop();
                            gridPanel.Controls.Remove(button);
                            button.Dispose();
                        }
                    }
                };
                animationTimer.Start(); // 开始动画
            }
        }

        private bool IsMatchInRow(int row, int col)
        {
            return grid[row, col] != null &&
                   grid[row, col] == grid[row, col + 1] &&
                   grid[row, col] == grid[row, col + 2];
        }

        private bool IsMatchInColumn(int col, int row)
        {
            return grid[row, col] != null &&
                   grid[row, col] == grid[row + 1, col] &&
                   grid[row, col] == grid[row + 2, col];
        }

        private void EliminateRowMatches(int row, int startCol, int count)
        {
            for (int i = 0; i < count; i++)
            {
                grid[row, startCol + i] = null; // 标记为null，表示消除
            }
        }

        private void EliminateColumnMatches(int col, int startRow, int count)
        {
            for (int i = 0; i < count; i++)
            {
                grid[startRow + i, col] = null; // 同上
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

        int firstx, firsty;
        int secondx, secondy;

        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            int buttonX = clickedButton.Location.X / buttonSize;
            int buttonY = clickedButton.Location.Y / buttonSize;

            if (firstClickedButton == null)
            {
                firstClickedButton = clickedButton;
                firstx = buttonX;
                firsty = buttonY;
            }
            else if (secondClickedButton == null && firstClickedButton != clickedButton)
            {
                secondClickedButton = clickedButton;
                secondx = buttonX;
                secondy = buttonY;

                if ((Math.Abs(firstx - secondx) == 1 && Math.Abs(firsty - secondy) == 0) ||
                    (Math.Abs(firstx - secondx) == 0 && Math.Abs(firsty - secondy) == 1))
                {
                    var tempLocation = firstClickedButton.Location;
                    firstClickedButton.Location = secondClickedButton.Location;
                    secondClickedButton.Location = tempLocation;

                    string tempGridValue = grid[firsty, firstx];
                    grid[firsty, firstx] = grid[secondy, secondx];
                    grid[secondy, secondx] = tempGridValue;

                    CheckAndEliminateMatches();
                }

                firstClickedButton = null;
                secondClickedButton = null;
                firstx = firsty = secondx = secondy = 0;
            }
        }
    }
}