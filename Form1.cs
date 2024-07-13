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
        private Label labelScore;
        private Label labelCountdown;
        private Button btnStart;
        private Button btnRestart;
        private Button btnNextMusic;
        private Button btnNextBackground;
        private Timer timerCountdown;

        private Panel gridPanel; // 8x8网格的Panel
        private int buttonSize;
        private int gridHeight;
        private int gridWidth;
        private int padding;
        private string[,] grid = new string[8, 8];
        private static string[] images;

        private bool isGameMusicStarted = false;
        private IWavePlayer waveOut;
        private Mp3FileReader mp3FileReader;
        private List<string> musicFiles = new List<string>();
        private int currentSongIndex = 0;
        private List<string> backgroundFiles = new List<string>();
        private int currentBackgroundIndex = 0;

        private int score = 0;
        private bool isGameStarted = false;

        private Button firstClickedButton;
        private Button secondClickedButton;
        int firstx, firsty;
        int secondx, secondy;

        private Timer swapTimer;
        private Button firstSwapButton;
        private Button secondSwapButton;
        private Point firstSwapButtonOriginalLocation;
        private Point secondSwapButtonOriginalLocation;
        private int swapStep = 5;
        private int swapSteps = 0;

        private int timeBoosterCount = 1;
        //private int scoreBoosterCount = 1;

        private bool isPaused = false;

        public Bejeweled()
        {
            InitializeComponent();

            musicFiles.Add("blackgroundmusic1.mp3");
            musicFiles.Add("blackgroundmusic2.mp3");
            musicFiles.Add("blackgroundmusic3.mp3");

            backgroundFiles.Add("background1.jpg");
            backgroundFiles.Add("background2.jpg");
            backgroundFiles.Add("background3.jpg");
            backgroundFiles.Add("background4.jpg");
            backgroundFiles.Add("background5.jpg");

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

            swapTimer = new Timer();
            swapTimer.Interval = 20;
            swapTimer.Tick += SwapTimer_Tick;
        }

        private void PlayMusic()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string musicPath = Path.Combine(currentDirectory, "..\\..\\Properties\\music\\blackgroundmusic1.mp3");

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

        private void PlayNextSong()
        {
            if (musicFiles.Count > 0)
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string musicPath = Path.Combine(currentDirectory, "..\\..\\Properties\\music\\" + musicFiles[currentSongIndex]);

                if (File.Exists(musicPath))
                {
                    mp3FileReader = new Mp3FileReader(musicPath);
                    waveOut = new WaveOutEvent();
                    waveOut.Init(mp3FileReader);
                    waveOut.Play();
                    isGameMusicStarted = true;
                }
                else
                {
                    MessageBox.Show("The MP3 file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
            labelScore = new Label
            {
                Text = "Score: 0",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            panel.Controls.Add(labelScore);

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
            btnStart = new Button
            {
                Text = "Start",
                Size = new Size(180, 50),
                Location = new Point(10, 70)
            };
            btnStart.Click += BtnStart_Click;
            panel.Controls.Add(btnStart);

            btnRestart = new Button
            {
                Text = "Restart",
                Size = new Size(180, 50),
                Location = new Point(10, 130)
            };
            btnRestart.Click += BtnRestart_Click;
            panel.Controls.Add(btnRestart);

            btnNextMusic = new Button
            {
                Text = "NextMusic",
                Size = new Size(180, 50),
                Location = new Point(10, 370)
            };
            btnNextMusic.Click += BtnNextMusict_Click;
            panel.Controls.Add(btnNextMusic);

            btnNextBackground = new Button
            {
                Text = "Nextblackground",
                Size = new Size(180, 50),
                Location = new Point(10, 430)
            };
            btnNextBackground.Click += BtnNextbackground_Click;
            panel.Controls.Add(btnNextBackground);

            Button btnTimeBooster = new Button
            {
                Text = "Time Booster",
                Size = new Size(60, 60),
                Location = new Point(10, 250)
            };
            btnTimeBooster.Click += BtnTimeBooster_Click;
            panel.Controls.Add(btnTimeBooster);

            Button btnScoreBooster = new Button
            {
                Text = "Score Booster",
                Size = new Size(60, 60),
                Location = new Point(80, 250)
            };
            //btnScoreBooster.Click += BtnScoreBooster_Click;
            panel.Controls.Add(btnScoreBooster);

            Button btnNull = new Button
            {
                Text = "Null",
                Size = new Size(40, 40),
                Location = new Point(150, 260)
            };
            panel.Controls.Add(btnNull);

            // 充值button
            Button btnRecharge = new Button
            {
                Text = "Recharge",
                Size = new Size(120, 40),
                Location = new Point(40, 320)
            };
            btnRecharge.Click += BtnRecharge_Click;
            panel.Controls.Add(btnRecharge);

            // 暂停button
            Button btnPause = new Button
            {
                Text = "Pause",
                Size = new Size(180, 50),
                Location = new Point(10, 190)
            };
            btnPause.Click += BtnPause_Click;
            panel.Controls.Add(btnPause);
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (isGameStarted && !isPaused)
            {
                timerCountdown.Stop();
                isPaused = true;

                DialogResult result = MessageBox.Show("Game is paused. Click '确定' to resume.", "Pause Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    ResumeGame();
                }
            }
        }

        private void ResumeGame()
        {
            if (isPaused)
            {
                timerCountdown.Start();
                isPaused = false;
            }
        }

        private void BtnRecharge_Click(object sender, EventArgs e)
        {
            string input = InputBox("Recharge", "Please enter the code to recharge:", "我是菜鸡");

            if (input == "我是菜鸡")
            {
                timeBoosterCount++;
                //scoreBoosterCount++;
                MessageBox.Show("Recharge successful! You have received 1 additional booster for each type.");
            }
            else
            {
                MessageBox.Show("Invalid code, recharge failed!");
            }
        }

        private string InputBox(string title, string promptText, string defaultValue)
        {
            Form inputForm = new Form
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label prompt = new Label
            {
                Left = 10,
                Top = 10,
                Width = 280,
                Text = promptText
            };

            TextBox inputBox = new TextBox
            {
                Left = 10,
                Top = 40,
                Width = 280,
                Text = defaultValue
            };

            Button confirmation = new Button
            {
                Left = 50,
                Top = 70,
                Width = 75,
                Text = "OK",
                DialogResult = DialogResult.OK
            };

            Button cancel = new Button
            {
                Left = 160,
                Top = 70,
                Width = 75,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            inputForm.Controls.Add(prompt);
            inputForm.Controls.Add(inputBox);
            inputForm.Controls.Add(confirmation);
            inputForm.Controls.Add(cancel);

            return inputForm.ShowDialog() == DialogResult.OK ? inputBox.Text : null;
        }

        private void BtnTimeBooster_Click(object sender, EventArgs e)
        {
            if (timeBoosterCount > 0)
            {
                timerCountdown.Interval = 1000;
                int newTimeLeft = 20 + int.Parse(labelCountdown.Text.Split(':')[1].Trim());
                labelCountdown.Text = $"Time: {newTimeLeft}";
                timeBoosterCount--;
            }
            else
            {
                MessageBox.Show("Time Booster is out of stock!");
            }
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
            labelCountdown.Text = "Time: 60";
            timerCountdown.Stop();
            UpdateScoreDisplay();
            ClearGrid();
            //InitializeGrid();
            StopMusic();
        }

        private void BtnNextMusict_Click(object sender, EventArgs e)
        {
            StopMusic();

            currentSongIndex = (currentSongIndex + 1) % musicFiles.Count;

            PlayNextSong();
        }

        private void BtnNextbackground_Click(object sender, EventArgs e)
        {
            currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundFiles.Count;

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string backgroundFilePath = Path.Combine(currentDirectory, "..\\..\\Properties\\background\\" + backgroundFiles[currentBackgroundIndex]);

            if (File.Exists(backgroundFilePath))
            {
                this.BackgroundImage = Image.FromFile(backgroundFilePath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                MessageBox.Show("Background image file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TimerCountdown_Tick(object sender, EventArgs e)
        {
            int timeLeft = int.Parse(labelCountdown.Text.Split(':')[1].Trim()) - 1;

            labelCountdown.Text = $"Time: {timeLeft}";

            if (timeLeft <= 0)
            {
                timerCountdown.Stop();
                isGameStarted = false;
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
                    ClearGrid();
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
            padding = 1;
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
            string lastImageName = null;

            if (images == null)
            {
                images = Enumerable.Range(1, numberOfImages)
                                   .Select(i => $"button_image{i}")
                                   .ToArray();
            }

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    string resourceName;
                    if (random.NextDouble() < 0.05 && lastImageName != null) // 有5%的概率使用上一个图片
                    {
                        resourceName = lastImageName;
                    }
                    else
                    {
                        resourceName = images[random.Next(images.Length)];
                        lastImageName = resourceName;
                    }

                    grid[row, col] = resourceName;

                    Image image = Properties.Resources.ResourceManager.GetObject(resourceName) as Image;

                    Button button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(col * (buttonSize + padding), row * (buttonSize + padding)),
                        BackColor = Color.Transparent
                    };

                    if (image != null)
                    {
                        button.BackgroundImageLayout = ImageLayout.Zoom;
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

                if (anyMatchFound)
                {
                    UpdateGridDisplay();
                    ApplyGravity();
                }

            } while (anyMatchFound);
        }

        private bool CanFindMatches()
        {
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth - 2; col++)
                {
                    if (grid[row, col] != null &&
                        grid[row, col] == grid[row, col + 1] &&
                        grid[row, col] == grid[row, col + 2])
                    {
                        return true;
                    }
                }
            }

            for (int col = 0; col < gridWidth; col++)
            {
                for (int row = 0; row < gridHeight - 2; row++)
                {
                    if (grid[row, col] != null &&
                        grid[row, col] == grid[row + 1, col] &&
                        grid[row, col] == grid[row + 2, col])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ApplyGravity()
        {
            for (int col = 0; col < gridWidth; col++)
            {
                for (int row = gridHeight - 1; row >= 0; row--)
                {
                    if (grid[row, col] == null)
                    {
                        for (int aboveRow = row - 1; aboveRow >= 0; aboveRow--)
                        {
                            if (grid[aboveRow, col] != null)
                            {
                                grid[row, col] = grid[aboveRow, col];
                                grid[aboveRow, col] = null;

                                Button buttonToMove = gridPanel.Controls.OfType<Button>()
                                    .FirstOrDefault(b => b.Location.Y / buttonSize == aboveRow && b.Location.X / buttonSize == col);
                                if (buttonToMove != null)
                                {
                                    buttonToMove.Location = new Point(col * (buttonSize + padding), row * (buttonSize + padding));
                                }
                                break;
                            }
                        }
                    }
                }
            }

            Random random = new Random();
            for (int col = 0; col < gridWidth; col++)
            {
                for (int row = 0; row < gridHeight; row++)
                {
                    if (grid[row, col] == null)
                    {
                        string resourceName = images[random.Next(images.Length)];
                        grid[row, col] = resourceName;

                        Image image = Properties.Resources.ResourceManager.GetObject(resourceName) as Image;

                        Button button = new Button
                        {
                            Size = new Size(buttonSize, buttonSize),
                            Location = new Point(col * (buttonSize + padding), row * (buttonSize + padding)),
                            BackColor = Color.Transparent
                        };

                        if (image != null)
                        {
                            button.BackgroundImageLayout = ImageLayout.Zoom;
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
        }

        private void PopulateGridWithButtons()
        {
            
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
                animationTimer.Start();
            }
        }

        private bool IsMatchInRow(int row, int col)
        {
            // 数组越界
            if (col >= gridWidth - 2) return false;

            return grid[row, col] != null &&
                   grid[row, col] == grid[row, col + 1] &&
                   grid[row, col] == grid[row, col + 2];
        }

        private bool IsMatchInColumn(int col, int row)
        {
            if (row >= gridHeight - 2) return false;

            return grid[row, col] != null &&
                   grid[row, col] == grid[row + 1, col] &&
                   grid[row, col] == grid[row + 2, col];
        }

        private void EliminateRowMatches(int row, int startCol, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // 从 gridPanel 中移除按钮
                foreach (Control control in gridPanel.Controls)
                {
                    if (control is Button button && button.Location.Y / buttonSize == row && button.Location.X / buttonSize == startCol + i)
                    {
                        gridPanel.Controls.Remove(button);
                        button.Dispose();
                        break;
                    }
                }

                grid[row, startCol + i] = null; // 标记为null，表示消除
            }
        }

        private void EliminateColumnMatches(int col, int startRow, int count)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (Control control in gridPanel.Controls)
                {
                    if (control is Button button && button.Location.Y / buttonSize == startRow + i && button.Location.X / buttonSize == col)
                    {
                        gridPanel.Controls.Remove(button);
                        button.Dispose();
                        break;
                    }
                }

                grid[startRow + i, col] = null;
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
                    firstSwapButton = firstClickedButton;
                    secondSwapButton = secondClickedButton;
                    firstSwapButtonOriginalLocation = firstSwapButton.Location;
                    secondSwapButtonOriginalLocation = secondSwapButton.Location;
                    swapSteps = 0;

                    swapTimer.Start();
                }
                else
                {
                    ResetClickedButtons();
                }
            }
        }

        private void SwapTimer_Tick(object sender, EventArgs e)
        {
            if (swapSteps < buttonSize / swapStep)
            {
                int stepX = (secondSwapButtonOriginalLocation.X - firstSwapButtonOriginalLocation.X) / (buttonSize / swapStep);
                int stepY = (secondSwapButtonOriginalLocation.Y - firstSwapButtonOriginalLocation.Y) / (buttonSize / swapStep);

                firstSwapButton.Location = new Point(firstSwapButton.Location.X + stepX, firstSwapButton.Location.Y + stepY);
                secondSwapButton.Location = new Point(secondSwapButton.Location.X - stepX, secondSwapButton.Location.Y - stepY);

                swapSteps++;
            }
            else
            {
                firstSwapButton.Location = secondSwapButtonOriginalLocation;
                secondSwapButton.Location = firstSwapButtonOriginalLocation;

                string tempGridValueA = grid[firsty, firstx];
                string tempGridValueB = grid[secondy, secondx];

                grid[firsty, firstx] = tempGridValueB;
                grid[secondy, secondx] = tempGridValueA;

                if (CanFindMatches())
                {
                    CheckAndEliminateMatches();
                    ApplyGravity();
                }
                else
                {
                    grid[firsty, firstx] = tempGridValueA;
                    grid[secondy, secondx] = tempGridValueB;
                    firstSwapButton.Location = firstSwapButtonOriginalLocation;
                    secondSwapButton.Location = secondSwapButtonOriginalLocation;
                }

                swapTimer.Stop();
                ResetClickedButtons();
            }
        }

        private void ResetClickedButtons()
        {
            firstClickedButton = null;
            secondClickedButton = null;
            firstx = firsty = secondx = secondy = 0;
        }
    }
}