using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace class11
{
    public partial class Form1 : Form
    {
        // ����GIFͼƬ����
        private Image gifImage;
        // ����Ƿ����ڲ��Ŷ���
        private bool isAnimating = false;
        // �洢����GIFͼ��λ��
        private List<Point> gifPositions = new List<Point>();
        // ��ȷָ��ʹ��Windows Forms��Timer
        private System.Windows.Forms.Timer resizeTimer;
        // GIFͼԭʼ��С
        private Size gifOriginalSize;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Size = new Size(600, 400);

            // ��ʼ����ʱ��
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 1000; // 1��
            resizeTimer.Tick += ResizeTimer_Tick;
            resizeTimer.Start();

            // �����С�ı�ʱ�����ػ�
            this.Resize += (s, e) => this.Invalidate();

            // ��ʼ���ı���Ĭ��ֵ����ѡ��
            textBox1.Text = "D:\\VisualStudio\\project\\class11\\èè��.gif";
        }

        // �ı������ݱ仯�¼�����ǰ����ʵ�ֹ��ܣ������գ�
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // �����Ҫʵʱ��Ӧ�ı��仯����������������߼�
            // ���磺����·��ʱ�Զ���֤��Ч��
        }

        // ��ť����¼������ģ����ز���ʾGIF��
        private async void button1_Click(object sender, EventArgs e)
        {
            string filePath = textBox1.Text.Trim();

            // ����֤·��
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("������GIF�ļ�·��");
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show("�ļ������ڣ�����·��");
                return;
            }

            // ���ð�ť��ֹ�ظ����
            button1.Enabled = false;
            button1.Text = "������...";

            try
            {
                // ʹ���첽���ط�ֹUI����
                await Task.Run(() => LoadGifFromPath(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("����ʧ�ܣ�" + ex.Message);
            }
            finally
            {
                // �ָ���ť״̬
                button1.Enabled = true;
                button1.Text = "Show";
            }
        }

        // ��·������GIF�ĺ��ķ���
        private void LoadGifFromPath(string filePath)
        {
            try
            {
                // �ͷ�ԭ��GIF��Դ
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

                // ������GIF
                gifImage = Image.FromFile(filePath);
                gifOriginalSize = gifImage.Size;

                // ���¼��㲼��
                RecalculateGifPositions();

                // ��������
                if (ImageAnimator.CanAnimate(gifImage))
                {
                    ImageAnimator.Animate(gifImage, (s, args) => this.Invalidate());
                    isAnimating = true;
                }

                // ǿ���ػ�
                this.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("����ʧ�ܣ�" + ex.Message);
            }
        }

        // ��ʱ���¼���ÿ���鴰���С�����²���
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            int previousCount = gifPositions.Count;
            RecalculateGifPositions();
            if (previousCount != gifPositions.Count)
            {
                this.Invalidate();
            }
        }

        // ��������GIF��λ�ã�����ԭʼ��С���Զ����У�
        private void RecalculateGifPositions()
        {
            if (gifImage == null) return;

            gifPositions.Clear();
            int clientWidth = this.ClientSize.Width;
            int clientHeight = this.ClientSize.Height;

            // ����������������ɵ�����
            int cols = Math.Max(1, clientWidth / gifOriginalSize.Width);
            int rows = Math.Max(1, clientHeight / gifOriginalSize.Height);

            // �������λ��
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

        // ��������GIF
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

        // �ͷ���Դ
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