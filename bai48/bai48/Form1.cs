using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bai48
{
    // ĐÃ SỬA TÊN CLASS THÀNH Form1 ĐỂ KHỚP VỚI Form1.Designer.cs
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnLoadData.Click += BtnLoadData_Click;
        }

        private async void BtnLoadData_Click(object? sender, EventArgs e)
        {
            btnLoadData.Enabled = false;
            progressBar1.Value = 0;
            lblPercent.Text = "0%";

            try
            {
                var progress = new Progress<int>(percent =>
                {
                    progressBar1.Value = percent;
                    lblPercent.Text = $"{percent}%";
                    this.Text = $"Đang nạp dữ liệu... {percent}%";
                });

                List<Student> result = await LoadStudentsAsync(progress);
                dgvStudents.DataSource = result;

                this.Text = "Quản lý Sinh viên - Hoàn tất";
                MessageBox.Show("Nạp dữ liệu hoàn tất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadData.Enabled = true;
            }
        }

        private async Task<List<Student>> LoadStudentsAsync(IProgress<int> progress)
        {
            return await Task.Run(() =>
            {
                List<Student> list = new List<Student>();
                int totalItems = 100;

                for (int i = 1; i <= totalItems; i++)
                {
                    Thread.Sleep(30);

                    list.Add(new Student
                    {
                        StudentId = $"SV{i:D3}",
                        FullName = $"Sinh viên {i}",
                        Age = 18 + (i % 5),
                        Gender = (i % 2 == 0) ? "Nữ" : "Nam"
                    });

                    int percentComplete = (i * 100) / totalItems;
                    progress?.Report(percentComplete);
                }

                return list;
            });
        }
    }

    public class Student
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
    }
}