namespace LiveSplit.LaMulanaRemake
{
    partial class ComponentSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.runMappingLayout = new System.Windows.Forms.TableLayoutPanel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.otherMappingLayout = new System.Windows.Forms.TableLayoutPanel();
            this.splitCondMenu = new System.Windows.Forms.ContextMenu();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(421, 422);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.runMappingLayout);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(413, 393);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Mappings (run)";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // runMappingLayout
            // 
            this.runMappingLayout.AutoScroll = true;
            this.runMappingLayout.ColumnCount = 2;
            this.runMappingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.runMappingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.runMappingLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runMappingLayout.Location = new System.Drawing.Point(3, 3);
            this.runMappingLayout.Name = "runMappingLayout";
            this.runMappingLayout.RowCount = 1;
            this.runMappingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.runMappingLayout.Size = new System.Drawing.Size(407, 387);
            this.runMappingLayout.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.otherMappingLayout);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(413, 393);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Mappings (other)";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // otherMappingLayout
            // 
            this.otherMappingLayout.AutoScroll = true;
            this.otherMappingLayout.ColumnCount = 2;
            this.otherMappingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.otherMappingLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.otherMappingLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.otherMappingLayout.Location = new System.Drawing.Point(3, 3);
            this.otherMappingLayout.Name = "otherMappingLayout";
            this.otherMappingLayout.RowCount = 1;
            this.otherMappingLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.otherMappingLayout.Size = new System.Drawing.Size(407, 387);
            this.otherMappingLayout.TabIndex = 0;
            // 
            // ComponentSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "ComponentSettings";
            this.Size = new System.Drawing.Size(421, 422);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TableLayoutPanel otherMappingLayout;
        private System.Windows.Forms.TableLayoutPanel runMappingLayout;
        private System.Windows.Forms.ContextMenu splitCondMenu;
    }
}
