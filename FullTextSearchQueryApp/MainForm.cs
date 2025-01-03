﻿// Copyright (c) 2020-2024 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using SoftCircuits.FullTextSearchQuery;

namespace FullTextSearchQueryApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Convert_Click(object sender, EventArgs e)
        {
            FtsQuery query = new(true);
            txtSqlQuery.Text = query.Transform(txtSearchTerm.Text);
        }
    }
}
