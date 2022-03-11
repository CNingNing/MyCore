namespace GIZConvert
{
    partial class Form2
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
            this.Radio1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Radio2 = new System.Windows.Forms.RadioButton();
            this.Radio3 = new System.Windows.Forms.RadioButton();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Radio1
            // 
            this.Radio1.AutoSize = true;
            this.Radio1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Radio1.Location = new System.Drawing.Point(18, 30);
            this.Radio1.Name = "Radio1";
            this.Radio1.Size = new System.Drawing.Size(47, 16);
            this.Radio1.TabIndex = 0;
            this.Radio1.TabStop = true;
            this.Radio1.Text = "红色";
            this.Radio1.UseVisualStyleBackColor = true;
            this.Radio1.CheckedChanged += new System.EventHandler(this.Radio_Change);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Radio2);
            this.groupBox1.Controls.Add(this.Radio3);
            this.groupBox1.Controls.Add(this.Radio1);
            this.groupBox1.Location = new System.Drawing.Point(28, 44);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(141, 132);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "文本框的前景色";
            // 
            // Radio2
            // 
            this.Radio2.AutoSize = true;
            this.Radio2.ForeColor = System.Drawing.SystemColors.InfoText;
            this.Radio2.Location = new System.Drawing.Point(18, 52);
            this.Radio2.Name = "Radio2";
            this.Radio2.Size = new System.Drawing.Size(47, 16);
            this.Radio2.TabIndex = 2;
            this.Radio2.TabStop = true;
            this.Radio2.Text = "蓝色";
            this.Radio2.UseVisualStyleBackColor = true;
            this.Radio2.CheckedChanged += new System.EventHandler(this.Radio_Change);
            // 
            // Radio3
            // 
            this.Radio3.AutoSize = true;
            this.Radio3.ForeColor = System.Drawing.SystemColors.MenuText;
            this.Radio3.Location = new System.Drawing.Point(18, 74);
            this.Radio3.Name = "Radio3";
            this.Radio3.Size = new System.Drawing.Size(47, 16);
            this.Radio3.TabIndex = 1;
            this.Radio3.TabStop = true;
            this.Radio3.Text = "黄色";
            this.Radio3.UseVisualStyleBackColor = true;
            this.Radio3.CheckedChanged += new System.EventHandler(this.Radio_Change);
            // 
            // richTextBox1
            // 
            this.richTextBox1.ForeColor = System.Drawing.Color.Black;
            this.richTextBox1.Location = new System.Drawing.Point(200, 44);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(114, 96);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(200, 153);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 248);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form2";
            this.Text = "Form2";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton Radio1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Radio2;
        private System.Windows.Forms.RadioButton Radio3;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
    }
}