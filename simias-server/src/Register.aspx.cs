using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
//using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// Summary description for ServerAdminForm.
	/// </summary>
	public class RegistrationForm : System.Web.UI.Page
	{
		[Ajax.Method]
		public string RegisterUser( string FirstName, string LastName, string UserName, string Password )
		{
			string status = "Successful";
			
			if ( UserName == null || UserName == "" || Password == null )
			{
                status = "Missing mandatory parameters";
			}
			else
			{
                RegistrationInfo info;
				Simias.Server.User user = new Simias.Server.User( UserName );
				user.FirstName = FirstName;
				user.LastName = LastName;
				//user.UserGuid = UserGuid;
				//user.FullName = FullName;
				//user.DN = DistinguishedName;
				//user.Email = Email;
			
				info = user.Create( Password );
                status = info.Status.ToString();
			}
			

			return status;
		}
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			Ajax.Manager.Register( this, "My.Page", Ajax.Debug.None );
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//

			/*
			this.FirstEdit.Text = "";
			this.LastEdit.Text = "";
			this.UserEdit.Text = "";
			this.PasswordEdit.Text = "";
			this.PwdVerifyEdit.Text = "";
			this.ErrorLabel.Text = "";
			this.RegisterButton.Enabled = true;
			*/

			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		/*
		private void RegisterButton_Click(object sender, System.EventArgs e)
		{
			if ( this.FirstEdit.Text == null || this.FirstEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a first name";
				return;
			}

			if ( this.LastEdit.Text == null || this.LastEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a last name";
				return;
			}

			if ( this.PasswordEdit.Text == null || this.PasswordEdit.Text == "" )
			{
				this.ErrorLabel.Text = "Please enter a password";
				return;
			}

			this.ErrorLabel.Text = "Registration successful";
			//this.RegisterButton.Enabled = false;
		}

		private void PasswordEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void FirstEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void LastEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
		
		}

		private void UserEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}

		private void PwdVerifyEdit_TextInit(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}

		private void PwdVerifyEdit_TextChanged(object sender, System.EventArgs e)
		{
			this.RegisterButton.Enabled = true;
			this.ErrorLabel.Text = "";
		
		}
		*/
	}
}
