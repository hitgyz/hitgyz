using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace class11
{
    public partial class Form1 : Form
    {
        // 定义GIF图片对象
        private Image gifImage;
        // 标记是否正在播放动画
        private bool isAnimating = false;
        // 存储所有GIF图的位置
        private List<Point> gifPositions = new List<Point>();
        // 明确指定使用Windows Forms的Timer
        private System.Windows.Forms.Timer resizeTimer;
        // GIF图原始大小
        private Size gifOriginalSize;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Size = new Size(600, 400);

            // 初始化计时器
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 1000; // 1秒
            resizeTimer.Tick += ResizeTimer_Tick;
            resizeTimer.Start();

            // 窗体大小改变时触发重绘
            this.Resize += (s, e) => this.Invalidate();

            // 初始化文本框默认值（可选）
            textBox1.Text = "D:\\VisualStudio\\project\\class11\\猫猫虫.gif";
        }

        // 文本框内容变化事件（当前无需实现功能，可留空）
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // 如果需要实时响应文本变化，可以在这里添加逻辑
            // 例如：输入路径时自动验证有效性
        }

        // 按钮点击事件（核心：加载并显示GIF）
        private async void button1_Click(object sender, EventArgs e)
        {
            string filePath = textBox1.Text.Trim();

            // 简单验证路径
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("请输入GIF文件路径");
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在，请检查路径");
                return;
            }

            // 禁用按钮防止重复点击
            button1.Enabled = false;
            button1.Text = "加载中...";

            try
            {
                // 使用异步加载防止UI卡顿
                await Task.Run(() => LoadGifFromPath(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败：" + ex.Message);
            }
            finally
            {
                // 恢复按钮状态
                button1.Enabled = true;
                button1.Text = "Show";
            }
        }

        // 从路径加载GIF的核心方法
        private void LoadGifFromPath(string filePath)
        {
            try
            {
                // 释放原有GIF资源
                if (gifImage != null)
                {
                    if (isAnimating)
                    {
                        ImageAnimator.StopAnimate(gifImage, null);
                    }
                    gifImage.Dispose();
                    gifImage = null;
                    isAnimating = false;
                }

                // 加载新GIF
                gifImage = Image.FromFile(filePath);
                gifOriginalSize = gifImage.Size;

                // 重新计算布局
                RecalculateGifPositions();

                // 启动动画
                if (ImageAnimator.CanAnimate(gifImage))
                {
                    ImageAnimator.Animate(gifImage, (s, args) => this.Invalidate());
                    isAnimating = true;
                }

                // 强制重绘
                this.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败：" + ex.Message);
            }
        }

        // 计时器事件：每秒检查窗体大小并更新布局
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            int previousCount = gifPositions.Count;
            RecalculateGifPositions();
            if (previousCount != gifPositions.Count)
            {
                this.Invalidate();
            }
        }

        // 计算所有GIF的位置（保持原始大小，自动排列）
        private void RecalculateGifPositions()
        {
            if (gifImage == null) return;

            gifPositions.Clear();
            int clientWidth = this.ClientSize.Width;
            int clientHeight = this.ClientSize.Height;

            // 计算横向和纵向可容纳的数量
            int cols = Math.Max(1, clientWidth / gifOriginalSize.Width);
            int rows = Math.Max(1, clientHeight / gifOriginalSize.Height);

            // 填充所有位置
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    gifPositions.Add(new Point(
                        x * gifOriginalSize.Width,
                        y * gifOriginalSize.Height
                    ));
                }
            }
        }

        // 绘制所有GIF
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (gifImage != null && isAnimating)
            {
                ImageAnimator.UpdateFrames();
                foreach (var pos in gifPositions)
                {
                    e.Graphics.DrawImage(gifImage, pos.X, pos.Y, gifOriginalSize.Width, gifOriginalSize.Height);
                }
            }
        }

        // 释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (resizeTimer != null)
            {
                resizeTimer.Stop();
                resizeTimer.Dispose();
            }
            if (gifImage != null)
            {
                if (isAnimating)
                {
                    ImageAnimator.StopAnimate(gifImage, null);
                }
                gifImage.Dispose();
            }
        }
    }
}