using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ImageSortApp
{
    // メインフォーム
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // ドラッグアンドドロップのエンターイベント
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // ドラッグアンドドロップのデータがファイルであるか確認
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // ドラッグアンドドロップのドロップイベント
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            // ドラッグアンドドロップされたファイルを取得
            if (e.Data != null)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 画像ファイルを取得
                var imageFiles = Directory.GetFiles(files[0], "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif")).ToArray();


                // 画像ファイルが存在する場合
                if (imageFiles.Length > 0)
                {
                    // 画像フォームを作成し、表示
                    var imageForm = new ImageForm(imageFiles, files[0]);
                    imageForm.Show();
                }
                else
                {
                    // 画像ファイルが存在しない場合、メッセージを表示
                    MessageBox.Show("画像ファイルがありません");
                }
            }
            else
            {
                MessageBox.Show("フォルダを確認にしてください。");
            }
        }
    }

    // 画像フォーム
    public class ImageForm : Form
    {
        private string[] imageFiles;
        private string sourceFolder;
        private int currentImageIndex = 0;
        private PictureBox pictureBox;
        private Button btnDelete;
        private Button btnSkip;
        private int skipFileIndex = 1;

        public ImageForm(string[] imageFiles, string sourceFolder)
        {
            // 初期化
            this.imageFiles = imageFiles;
            this.sourceFolder = sourceFolder;

            // PictureBoxを作成
            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);

            // 削除ボタンを作成
            this.btnDelete = new Button { Text = "Delete", Dock = DockStyle.Bottom, Height = 50 };
            this.btnDelete.Click += btnDelete_Click;
            this.Controls.Add(btnDelete);

            // スキップボタンを作成
            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);

            // 移動先のフォルダを作成
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            if (Directory.Exists(destinationFolder))
            {
                // 移動先のフォルダ内のファイルを取得
                var existingFiles = Directory.GetFiles(destinationFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.StartsWith(Path.GetFileName(sourceFolder)));

                // 移動先のフォルダ内にファイルが存在する場合
                if (existingFiles.Any())
                {
                    // 最後のファイル名を取得
                    var lastFileName = existingFiles.OrderBy(f => f).Last();

                    // 最後のファイル名から連番を取得
                    var lastFileNumber = int.Parse(Path.GetFileNameWithoutExtension(lastFileName).Substring(Path.GetFileName(sourceFolder).Length, 4));

                    // 連番を更新
                    skipFileIndex = lastFileNumber + 1;
                }
            }

            // 現在の画像を表示
            ShowCurrentImage();
        }

        // 現在の画像を表示
        private void ShowCurrentImage()
        {
            // 画像ファイルが存在する場合
            if (currentImageIndex < imageFiles.Length)
            {
                // 画像を読み込み、表示
                using (var img = new Bitmap(imageFiles[currentImageIndex]))
                {
                    pictureBox.Image = new Bitmap(img);
                }
            }
            else
            {
                // 画像ファイルが存在しない場合、メッセージを表示し、フォームを閉じる
                MessageBox.Show("画像ファイルがありません");
                this.Close();
            }
        }

        // 削除ボタンのクリックイベント
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 現在の画像ファイルを削除
            File.Delete(imageFiles[currentImageIndex]);

            // 次の画像へ
            currentImageIndex++;

            // 画像を表示
            ShowCurrentImage();
        }

        // スキップボタンのクリックイベント
        private void btnSkip_Click(object sender, EventArgs e)
        {
            // 移動先のフォルダを作成
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            Directory.CreateDirectory(destinationFolder);

            // 新しいファイル名を作成
            var fileName = Path.GetFileName(imageFiles[currentImageIndex]);
            var newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
            var destinationPath = Path.Combine(destinationFolder, newFileName);

            // 同じ名前のファイルが存在する場合は、連番を更新
            while (File.Exists(destinationPath))
            {
                skipFileIndex++;
                newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
                destinationPath = Path.Combine(destinationFolder, newFileName);
            }

            // ファイルを移動
            File.Move(imageFiles[currentImageIndex], destinationPath);

            // 次の画像へ
            currentImageIndex++;

            // 連番を更新
            skipFileIndex++;

            // 画像を表示
            ShowCurrentImage();
        }
    }
}
