namespace OptikaKargin
{
    partial class FormProduct
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProduct));
            this.buttonadd = new System.Windows.Forms.Button();
            this.buttonredactirovanie = new System.Windows.Forms.Button();
            this.buttondelete = new System.Windows.Forms.Button();
            this.buttonBask = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.comboBoxFiltr = new System.Windows.Forms.ComboBox();
            this.comboBoxSort = new System.Windows.Forms.ComboBox();
            this.textBoxPoick = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonadd
            // 
            this.buttonadd.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonadd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonadd.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonadd.Location = new System.Drawing.Point(869, 535);
            this.buttonadd.Margin = new System.Windows.Forms.Padding(5);
            this.buttonadd.Name = "buttonadd";
            this.buttonadd.Size = new System.Drawing.Size(133, 51);
            this.buttonadd.TabIndex = 0;
            this.buttonadd.Text = "Добавить";
            this.buttonadd.UseVisualStyleBackColor = false;
            this.buttonadd.Click += new System.EventHandler(this.buttonadd_Click);
            // 
            // buttonredactirovanie
            // 
            this.buttonredactirovanie.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonredactirovanie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonredactirovanie.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonredactirovanie.Location = new System.Drawing.Point(667, 535);
            this.buttonredactirovanie.Margin = new System.Windows.Forms.Padding(5);
            this.buttonredactirovanie.Name = "buttonredactirovanie";
            this.buttonredactirovanie.Size = new System.Drawing.Size(192, 51);
            this.buttonredactirovanie.TabIndex = 1;
            this.buttonredactirovanie.Text = "Редактировать";
            this.buttonredactirovanie.UseVisualStyleBackColor = false;
            this.buttonredactirovanie.Click += new System.EventHandler(this.buttonredactirovanie_Click);
            // 
            // buttondelete
            // 
            this.buttondelete.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttondelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttondelete.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttondelete.Location = new System.Drawing.Point(524, 535);
            this.buttondelete.Margin = new System.Windows.Forms.Padding(5);
            this.buttondelete.Name = "buttondelete";
            this.buttondelete.Size = new System.Drawing.Size(133, 51);
            this.buttondelete.TabIndex = 2;
            this.buttondelete.Text = "Удалить";
            this.buttondelete.UseVisualStyleBackColor = false;
            this.buttondelete.Click += new System.EventHandler(this.buttondelete_Click);
            // 
            // buttonBask
            // 
            this.buttonBask.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonBask.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonBask.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonBask.Location = new System.Drawing.Point(6, 535);
            this.buttonBask.Margin = new System.Windows.Forms.Padding(5);
            this.buttonBask.Name = "buttonBask";
            this.buttonBask.Size = new System.Drawing.Size(137, 51);
            this.buttonBask.TabIndex = 3;
            this.buttonBask.Text = "Назад";
            this.buttonBask.UseVisualStyleBackColor = false;
            this.buttonBask.Click += new System.EventHandler(this.buttonBask_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 75);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 67;
            this.dataGridView1.Size = new System.Drawing.Size(995, 407);
            this.dataGridView1.TabIndex = 4;
            // 
            // comboBoxFiltr
            // 
            this.comboBoxFiltr.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.comboBoxFiltr.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxFiltr.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.comboBoxFiltr.FormattingEnabled = true;
            this.comboBoxFiltr.Location = new System.Drawing.Point(657, 30);
            this.comboBoxFiltr.Name = "comboBoxFiltr";
            this.comboBoxFiltr.Size = new System.Drawing.Size(167, 39);
            this.comboBoxFiltr.TabIndex = 5;
            this.comboBoxFiltr.Text = "Фильтрация";
            this.comboBoxFiltr.SelectedIndexChanged += new System.EventHandler(this.comboBoxFiltr_SelectedIndexChanged);
            // 
            // comboBoxSort
            // 
            this.comboBoxSort.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.comboBoxSort.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comboBoxSort.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.comboBoxSort.FormattingEnabled = true;
            this.comboBoxSort.Location = new System.Drawing.Point(830, 30);
            this.comboBoxSort.Name = "comboBoxSort";
            this.comboBoxSort.Size = new System.Drawing.Size(171, 39);
            this.comboBoxSort.TabIndex = 6;
            this.comboBoxSort.Text = "Сортировка";
            this.comboBoxSort.SelectedIndexChanged += new System.EventHandler(this.comboBoxSort_SelectedIndexChanged);
            // 
            // textBoxPoick
            // 
            this.textBoxPoick.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBoxPoick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPoick.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxPoick.Location = new System.Drawing.Point(14, 30);
            this.textBoxPoick.Name = "textBoxPoick";
            this.textBoxPoick.Size = new System.Drawing.Size(213, 38);
            this.textBoxPoick.TabIndex = 7;
            this.textBoxPoick.TextChanged += new System.EventHandler(this.textBoxPoick_TextChanged_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(151, 555);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 31);
            this.label1.TabIndex = 8;
            this.label1.Text = "label1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 488);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 30);
            this.button1.TabIndex = 9;
            this.button1.Text = "<";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(60, 488);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(48, 30);
            this.button2.TabIndex = 10;
            this.button2.Text = ">";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(150, 488);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 31);
            this.label2.TabIndex = 11;
            this.label2.Text = "label2";
            // 
            // FormProduct
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::OptikaKargin.Properties.Resources._123;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1016, 604);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxPoick);
            this.Controls.Add(this.comboBoxSort);
            this.Controls.Add(this.comboBoxFiltr);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.buttonBask);
            this.Controls.Add(this.buttondelete);
            this.Controls.Add(this.buttonredactirovanie);
            this.Controls.Add(this.buttonadd);
            this.Font = new System.Drawing.Font("Comic Sans MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FormProduct";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Product";
            this.Load += new System.EventHandler(this.FormProduct_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonadd;
        private System.Windows.Forms.Button buttonredactirovanie;
        private System.Windows.Forms.Button buttondelete;
        private System.Windows.Forms.Button buttonBask;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ComboBox comboBoxFiltr;
        private System.Windows.Forms.ComboBox comboBoxSort;
        private System.Windows.Forms.TextBox textBoxPoick;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
    }
}