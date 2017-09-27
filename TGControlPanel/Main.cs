﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace TGControlPanel
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
			Panels.SelectedIndexChanged += Panels_SelectedIndexChanged;
			Panels.SelectedIndex += Math.Min(Properties.Settings.Default.LastPageIndex, Panels.TabCount - 1);
			InitRepoPage();
			InitBYONDPage();
			InitServerPage();
			LoadChatPage();
			InitStaticPage();
		}

		private void Main_Resize(object sender, EventArgs e)
		{
			Panels.Location = new Point(10, 10);
			Panels.Width = ClientSize.Width - 20;
			Panels.Height = ClientSize.Height - 20;
		}

		private void Panels_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.LastPageIndex = Panels.SelectedIndex;
		}
	}
}
