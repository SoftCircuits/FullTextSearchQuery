//////////////////////////////////////////////////////////////////////////////
// Easy Full Text Search .NET Class Library
//
// This source code and all associated files and resources are copyrighted by
// the author(s). This source code and all associated files and resources may
// be used as long as they are used according to the terms and conditions set
// forth in The Code Project Open License (CPOL).
//
// Copyright (c) 2015 Jonathan Wood
// http://www.softcircuits.com
// http://www.blackbeltcoder.com
//

using System;
using System.Windows.Forms;
using FullTextSearchQuery;

namespace TestFullTextSearchQuery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            FtsQuery query = new FtsQuery(true);
            txtSqlQuery.Text = query.Transform(txtSearchTerm.Text);
        }
    }
}
