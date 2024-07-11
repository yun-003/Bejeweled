using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bejeweled
{
    public partial class Bejeweled : Form
    {
        public Bejeweled()
        {
            InitializeComponent();

            //InitializeGrid();

            // 创建FlowLayoutPanel用于放置控制元素
            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200, // 控制面板的宽度
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

        private void UpdateScoreDisplay()
        {
            labelScore.Text = $"Score: {score}";
        }

        private void InitializeLabels(Panel panel)
        {
            // 添加分数标签
            labelScore = new Label
            {
                Text = "Score: 0",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            panel.Controls.Add(labelScore);

            // 添加倒计时标签
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
            // 添加开始按钮
            btnStart = new Button
            {
                Text = "Start",
                Size = new Size(180, 50),
                Location = new Point(10, 70)
            };
            btnStart.Click += BtnStart_Click;
            panel.Controls.Add(btnStart);

            // 添加重新开始按钮
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
                isGameStarted = true; // 标记游戏已经开始
                InitializeGrid();
            }
            else
            {
                // 游戏已经开始，弹出消息框
                MessageBox.Show("Game has already started!");
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            isGameStarted = false; // 重置游戏开始状态
            score = 0;
            labelCountdown.Text = "Time: 60";
            timerCountdown.Stop();
            UpdateScoreDisplay();
            ClearGrid(); // 重新开始时清除网格
            InitializeGrid(); // 然后重新初始化网格
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

            int gridPanelWidth = (buttonSize + padding) * gridWidth - padding;
            int gridPanelHeight = gridPanelWidth;

            gridPanel = new Panel
            {
                Size = new Size(gridPanelWidth, gridPanelHeight),
                Location = new Point(200 + padding, (this.ClientSize.Height - gridPanelHeight) / 2),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(gridPanel);

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    Button button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(col * (buttonSize + padding), row * (buttonSize + padding)),
                        Text = "Button"
                    };
                    gridPanel.Controls.Add(button);
                    // 可以为按钮添加事件处理，例如点击事件
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