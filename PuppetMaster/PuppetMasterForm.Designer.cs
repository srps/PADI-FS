namespace PuppetMaster
{
    partial class PuppetMasterForm
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
            this.load_script = new System.Windows.Forms.Button();
            this.run_script = new System.Windows.Forms.Button();
            this.next_step_script = new System.Windows.Forms.Button();
            this.puppet_master_history_TextBox = new System.Windows.Forms.TextBox();
            this.History = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dump_history_TextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog_PADI_FS = new System.Windows.Forms.OpenFileDialog();
            this.run_command = new System.Windows.Forms.Button();
            this.command_TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // load_script
            // 
            this.load_script.Location = new System.Drawing.Point(690, 54);
            this.load_script.Name = "load_script";
            this.load_script.Size = new System.Drawing.Size(146, 33);
            this.load_script.TabIndex = 0;
            this.load_script.Text = "Load Script";
            this.load_script.UseVisualStyleBackColor = true;
            this.load_script.Click += new System.EventHandler(this.load_script_Click);
            // 
            // run_script
            // 
            this.run_script.Location = new System.Drawing.Point(690, 105);
            this.run_script.Name = "run_script";
            this.run_script.Size = new System.Drawing.Size(146, 33);
            this.run_script.TabIndex = 1;
            this.run_script.Text = "Run Loaded Script";
            this.run_script.UseVisualStyleBackColor = true;
            this.run_script.Click += new System.EventHandler(this.run_script_Click);
            // 
            // next_step_script
            // 
            this.next_step_script.Location = new System.Drawing.Point(690, 163);
            this.next_step_script.Name = "next_step_script";
            this.next_step_script.Size = new System.Drawing.Size(146, 33);
            this.next_step_script.TabIndex = 2;
            this.next_step_script.Text = "Next Step in Loaded Script";
            this.next_step_script.UseVisualStyleBackColor = true;
            this.next_step_script.Click += new System.EventHandler(this.next_step_script_Click);
            // 
            // puppet_master_history_TextBox
            // 
            this.puppet_master_history_TextBox.Location = new System.Drawing.Point(12, 54);
            this.puppet_master_history_TextBox.Multiline = true;
            this.puppet_master_history_TextBox.Name = "puppet_master_history_TextBox";
            this.puppet_master_history_TextBox.Size = new System.Drawing.Size(421, 375);
            this.puppet_master_history_TextBox.TabIndex = 3;
            // 
            // History
            // 
            this.History.AutoSize = true;
            this.History.Location = new System.Drawing.Point(12, 23);
            this.History.Name = "History";
            this.History.Size = new System.Drawing.Size(111, 13);
            this.History.TabIndex = 4;
            this.History.Text = "Puppet Master History";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(454, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Dump History";
            // 
            // dump_history_TextBox
            // 
            this.dump_history_TextBox.Location = new System.Drawing.Point(457, 54);
            this.dump_history_TextBox.Multiline = true;
            this.dump_history_TextBox.Name = "dump_history_TextBox";
            this.dump_history_TextBox.Size = new System.Drawing.Size(227, 320);
            this.dump_history_TextBox.TabIndex = 6;
            // 
            // openFileDialog_PADI_FS
            // 
            this.openFileDialog_PADI_FS.FileName = "openFileDialog1";
            // 
            // run_command
            // 
            this.run_command.Location = new System.Drawing.Point(690, 397);
            this.run_command.Name = "run_command";
            this.run_command.Size = new System.Drawing.Size(146, 31);
            this.run_command.TabIndex = 7;
            this.run_command.Text = "Run Command";
            this.run_command.UseVisualStyleBackColor = true;
            this.run_command.Click += new System.EventHandler(this.run_command_Click);
            // 
            // command_TextBox
            // 
            this.command_TextBox.Location = new System.Drawing.Point(457, 397);
            this.command_TextBox.Multiline = true;
            this.command_TextBox.Name = "command_TextBox";
            this.command_TextBox.Size = new System.Drawing.Size(227, 32);
            this.command_TextBox.TabIndex = 8;
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 441);
            this.Controls.Add(this.command_TextBox);
            this.Controls.Add(this.run_command);
            this.Controls.Add(this.dump_history_TextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.History);
            this.Controls.Add(this.puppet_master_history_TextBox);
            this.Controls.Add(this.next_step_script);
            this.Controls.Add(this.run_script);
            this.Controls.Add(this.load_script);
            this.Name = "PuppetMasterForm";
            this.Text = "PuppetMasterForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button load_script;
        private System.Windows.Forms.Button run_script;
        private System.Windows.Forms.Button next_step_script;
        private System.Windows.Forms.TextBox puppet_master_history_TextBox;
        private System.Windows.Forms.Label History;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox dump_history_TextBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog_PADI_FS;
        private System.Windows.Forms.Button run_command;
        private System.Windows.Forms.TextBox command_TextBox;
    }
}

