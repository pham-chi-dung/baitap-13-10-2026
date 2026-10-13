using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace bai46
{
    public partial class Form1 : Form
    {
        // Sử dụng BindingList để hỗ trợ cập nhật giao diện tự động khi danh sách thay đổi
        private BindingList<Student> _studentList;
        private BindingSource _bindingSource;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            _studentList = new BindingList<Student>();
            _bindingSource = new BindingSource { DataSource = _studentList };

            // Kết nối DataGridView với BindingSource
            dgvStudents.DataSource = _bindingSource;

            // Khởi tạo dữ liệu ComboBox Giới tính
            cboGender.Items.AddRange(new string[] { "Nam", "Nữ" });

            // THIẾT LẬP DATA BINDING 2 CHIỀU
            txtStudentId.DataBindings.Add("Text", _bindingSource, "StudentId", true, DataSourceUpdateMode.OnPropertyChanged);
            txtFullName.DataBindings.Add("Text", _bindingSource, "FullName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", _bindingSource, "Age", true, DataSourceUpdateMode.OnPropertyChanged);
            cboGender.DataBindings.Add("Text", _bindingSource, "Gender", true, DataSourceUpdateMode.OnPropertyChanged);

            // Bắt sự kiện khi chuyển dòng trên DataGridView để nạp ảnh
            _bindingSource.CurrentChanged += BindingSource_CurrentChanged;

            // Gán sự kiện cho các nút
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnChooseImage.Click += BtnChooseImage_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
        }

        // Tự động nạp PictureBox khi chọn một dòng khác trên DataGridView
        private void BindingSource_CurrentChanged(object? sender, EventArgs e)
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                LoadAvatar(currentStudent.AvatarPath);
            }
            else
            {
                picAvatar.Image = null;
            }
        }

        private void LoadAvatar(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    picAvatar.Image = Image.FromFile(path);
                }
                else
                {
                    picAvatar.Image = null;
                }
            }
            catch
            {
                picAvatar.Image = null; // Tránh crash nếu file ảnh bị lỗi
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            // Thêm một sinh viên rỗng vào danh sách để người dùng bắt đầu nhập liệu qua Binding
            var newStudent = new Student();
            _studentList.Add(newStudent);
            _bindingSource.Position = _studentList.Count - 1;
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            // Do đã dùng DataBindings (2 chiều) cập nhật OnPropertyChanged, 
            // dữ liệu trên lưới tự động thay đổi theo TextBox.
            // Chỉ cần Refresh lại lưới để chắc chắn hiển thị cập nhật mượt mà.
            dgvStudents.Refresh();
            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                DialogResult result = MessageBox.Show($"Xóa sinh viên: {currentStudent.FullName}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    _bindingSource.RemoveCurrent();
                }
            }
        }

        private void BtnChooseImage_Click(object? sender, EventArgs e)
        {
            if (_bindingSource.Current is Student currentStudent)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Chọn ảnh đại diện";
                    ofd.Filter = "Image Files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        // Gán đường dẫn vào sinh viên đang chọn
                        currentStudent.AvatarPath = ofd.FileName;

                        // Hiển thị lên PictureBox
                        LoadAvatar(currentStudent.AvatarPath);

                        // Cập nhật lại Grid
                        dgvStudents.Refresh();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn hoặc thêm một sinh viên trước khi chọn ảnh!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            if (_studentList.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Xuất danh sách sinh viên";
                sfd.Filter = "CSV File (*.csv)|*.csv";
                sfd.FileName = "DanhSachSinhVien.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Hỗ trợ tiếng Việt UTF-8
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            // Ghi tiêu đề cột (phân cách bằng dấu phẩy)
                            sw.WriteLine("StudentId,FullName,Age,Gender,AvatarPath");

                            // Ghi từng dòng dữ liệu
                            foreach (var student in _studentList)
                            {
                                // Xử lý trường hợp tên có chứa dấu phẩy để không bị lệch cột CSV
                                string name = student.FullName.Contains(",") ? $"\"{student.FullName}\"" : student.FullName;
                                sw.WriteLine($"{student.StudentId},{name},{student.Age},{student.Gender},{student.AvatarPath}");
                            }
                        }
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    // Cấu trúc dữ liệu lớp Student
    public class Student
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
    }
}