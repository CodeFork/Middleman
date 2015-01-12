namespace Middleman.Service
{
    partial class ProjectInstaller
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
            this.middlemanProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.middlemanInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // middlemanProcessInstaller
            // 
            this.middlemanProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.middlemanProcessInstaller.Password = null;
            this.middlemanProcessInstaller.Username = null;
            // 
            // middlemanInstaller
            // 
            this.middlemanInstaller.Description = "Middleman";
            this.middlemanInstaller.DisplayName = "Middleman";
            this.middlemanInstaller.ServiceName = "Middleman";
            this.middlemanInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.middlemanProcessInstaller,
            this.middlemanInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller middlemanProcessInstaller;
        private System.ServiceProcess.ServiceInstaller middlemanInstaller;
    }
}