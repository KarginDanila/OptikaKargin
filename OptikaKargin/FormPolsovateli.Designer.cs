﻿namespace OptikaKargin
{
    partial class FormPolsovateli
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPolsovateli));
            this.textBoxPoick = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buttonBask = new System.Windows.Forms.Button();
            this.buttonredactirovanie = new System.Windows.Forms.Button();
            this.buttonadd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxPoick
            // 
            this.textBoxPoick.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBoxPoick.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxPoick.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxPoick.Location = new System.Drawing.Point(10, 17);
            this.textBoxPoick.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPoick.Name = "textBoxPoick";
            this.textBoxPoick.Size = new System.Drawing.Size(217, 38);
            this.textBoxPoick.TabIndex = 21;
            this.textBoxPoick.TextChanged += new System.EventHandler(this.textBoxPoick_TextChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ActiveBorder;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 73);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 70;
            this.dataGridView1.Size = new System.Drawing.Size(982, 443);
            this.dataGridView1.TabIndex = 20;
            // 
            // buttonBask
            // 
            this.buttonBask.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonBask.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonBask.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonBask.Location = new System.Drawing.Point(10, 526);
            this.buttonBask.Margin = new System.Windows.Forms.Padding(6);
            this.buttonBask.Name = "buttonBask";
            this.buttonBask.Size = new System.Drawing.Size(152, 51);
            this.buttonBask.TabIndex = 19;
            this.buttonBask.Text = "Назад";
            this.buttonBask.UseVisualStyleBackColor = false;
            this.buttonBask.Click += new System.EventHandler(this.buttonBask_Click);
            // 
            // buttonredactirovanie
            // 
            this.buttonredactirovanie.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonredactirovanie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonredactirovanie.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonredactirovanie.Location = new System.Drawing.Point(638, 526);
            this.buttonredactirovanie.Margin = new System.Windows.Forms.Padding(6);
            this.buttonredactirovanie.Name = "buttonredactirovanie";
            this.buttonredactirovanie.Size = new System.Drawing.Size(211, 51);
            this.buttonredactirovanie.TabIndex = 17;
            this.buttonredactirovanie.Text = "Редактировать";
            this.buttonredactirovanie.UseVisualStyleBackColor = false;
            this.buttonredactirovanie.Click += new System.EventHandler(this.buttonredactirovanie_Click);
            // 
            // buttonadd
            // 
            this.buttonadd.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.buttonadd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonadd.Font = new System.Drawing.Font("Segoe Script", 14.25F, System.Drawing.FontStyle.Bold);
            this.buttonadd.Location = new System.Drawing.Point(861, 526);
            this.buttonadd.Margin = new System.Windows.Forms.Padding(6);
            this.buttonadd.Name = "buttonadd";
            this.buttonadd.Size = new System.Drawing.Size(133, 51);
            this.buttonadd.TabIndex = 16;
            this.buttonadd.Text = "Добавить";
            this.buttonadd.UseVisualStyleBackColor = false;
            this.buttonadd.Click += new System.EventHandler(this.buttonadd_Click);
            // 
            // FormPolsovateli
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::OptikaKargin.Properties.Resources._123;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1007, 595);
            this.Controls.Add(this.textBoxPoick);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.buttonBask);
            this.Controls.Add(this.buttonredactirovanie);
            this.Controls.Add(this.buttonadd);
            this.Font = new System.Drawing.Font("Comic Sans MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FormPolsovateli";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormPolsovateli";
            this.Load += new System.EventHandler(this.FormPolsovateli_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxPoick;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button buttonBask;
        private System.Windows.Forms.Button buttonredactirovanie;
        private System.Windows.Forms.Button buttonadd;
    }
}