﻿using System;
using System.ComponentModel;
using System.Windows.Forms;


using System.Diagnostics;

namespace zNevis
{
   public partial class Form1 : Form
   {
      #region Fields
      MPlayerController mplayer;
      SrtSubtitle srt;
      #endregion

      public Form1()
      {
         InitializeComponent();
      }


      private void Form1_Load(object sender, EventArgs e)
      {
         mplayer = new MPlayerController(mplayerPanel.Handle.ToString());
         mplayer.OnOutputReceived += (s) =>
            {
               Console.WriteLine(s);
            }
         ;

         mplayer.OnErrorReceived += (s) =>
            {
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine(s);
               Console.ResetColor();
            }
         ;

         mplayer.VideoLengthChanged += (length) =>
            {
               lengthBar.Invoke(new Action(() => { lengthBar.Maximum = (int)length; lengthBar.Value = lengthBar.Maximum / 2; }));

               //lengthBar.Maximum = (int)length;

               Console.WriteLine("new length:::::" + length);
            }
         ;

         mplayer.VideoPositionChanged += (pos) =>
            {
               lengthBar.Invoke(new Action(() =>
               {
                  lengthBar.Value = (int)pos;
               }));
            }
         ;
      }


      private void openToolStripButton_Click(object sender, EventArgs e)
      {
         if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
         {
            mplayer.LoadFile(openFileDialog1.FileName);
         }
      }

      private void newToolButton_Click(object sender, EventArgs e)
      {
         var sfd = new SaveFileDialog();
         if (sfd.ShowDialog() != DialogResult.OK)
            return;
         srt = SrtSubtitle.CreateNew(sfd.FileName, false);
      }

      private void mplayerPanel_Click(object sender, EventArgs e)
      {
         mplayer.PlayPause();
      }

      private void lengthBar_MouseUp(object sender, EventArgs e)
      {
      }

      private void lengthBar_MouseDown(object sender, MouseEventArgs e)
      {
         mplayer.Pause();
      }

      private void lengthBar_MouseCaptureChanged(object sender, EventArgs e)
      {
         mplayer.Seek(lengthBar.Value);
         //mplayer.PlayPause();
      }
   }
}
