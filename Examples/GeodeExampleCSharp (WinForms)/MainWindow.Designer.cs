
namespace GeodeExampleCSharp
{
    partial class MainWindow
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.SendAlertButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SendAlertButton
            // 
            this.SendAlertButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SendAlertButton.Location = new System.Drawing.Point(0, 0);
            this.SendAlertButton.Name = "SendAlertButton";
            this.SendAlertButton.Size = new System.Drawing.Size(282, 153);
            this.SendAlertButton.TabIndex = 0;
            this.SendAlertButton.Text = "Send alert";
            this.SendAlertButton.UseVisualStyleBackColor = true;
            this.SendAlertButton.Click += new System.EventHandler(this.SendAlertButton_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 153);
            this.Controls.Add(this.SendAlertButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Example";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SendAlertButton;
    }
}

