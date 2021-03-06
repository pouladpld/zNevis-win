﻿using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace zNevis
{
   class MPlayerController
   {
      #region Fields
      Process _p = null;
      bool _isIdentifying = false;
      Regex _idTagRegex = new Regex(@"ID_(?<tag>[A-Z_\d]+)=(?<value>[A-Z\d\.:\\]+)");
      Regex _positionRegex = new Regex(@"V:\s+(?<pos>\d+\.\d+)\s+");
      Dictionary<string, string> _mediaIdDict = new Dictionary<string, string>();
      #endregion

      #region Properties
      public string FileName { set; get; }
      public double Position { get; private set; }
      #endregion

      #region Events
      public delegate void OnDataReceived(string s);
      public event OnDataReceived OnOutputReceived;
      public event OnDataReceived OnErrorReceived;

      public delegate void NumericIdNotifier(double length);
      public event NumericIdNotifier VideoLengthChanged;
      public event NumericIdNotifier VideoPositionChanged;
      #endregion

      public MPlayerController(string wid)
      {
         _p = new Process();
         _p.StartInfo.FileName = "mplayer.exe";
         _p.StartInfo.Arguments = @"-slave -idle -identify -osdlevel 0 -noconfig all -noautosub -nocookies -noborder -ass -wid " + wid;
         _p.StartInfo.CreateNoWindow = true;
         _p.StartInfo.UseShellExecute = false;
         _p.StartInfo.RedirectStandardInput = true;
         _p.StartInfo.RedirectStandardOutput = true;
         _p.StartInfo.RedirectStandardError = true;
         _p.OutputDataReceived += ReadStdOut;
         _p.ErrorDataReceived += ReadStdErr;
         _p.Start();
         _p.BeginErrorReadLine();
         _p.BeginOutputReadLine();
      }

      public void PlayPause()
      {
         _p.StandardInput.Write("p\n");
      }

      public void Pause()
      {
         _p.StandardInput.Write("pausing_keep_force pause\n");
      }

      public void Seek(double toPos)
      {
         _p.StandardInput.Write("pausing_keep_force seek {0} 2\npause\npausing_keep_force pause\n", toPos);
      }

      /// <summary>
      /// Loads file from path into the mplayer and starts playing
      /// </summary>
      /// <param name="path"></param>
      public void LoadFile(string path)
      {
         if (!File.Exists(path))
            return;
         string command = string.Format("pausing_keep_force loadfile \"{0}\" 0\n", path.Replace(@"\", @"\\"));
         _isIdentifying = true;
         _p.StandardInput.Write(command);
      }

      private void ReadStdOut(object sender, DataReceivedEventArgs e)
      {
         if (_isIdentifying)
         {
            ReadStdOutId(e.Data);
         }
         else if (_positionRegex.IsMatch(e.Data))
         {
            double pos;
            if (double.TryParse(_positionRegex.Match(e.Data).Groups[1].Value, out pos))
            {
               Position = pos;
            }
            VideoPositionChanged(Position);
         }

         OnOutputReceived(e.Data);
      }

      private void ReadStdOutId(string output)
      {
         if (output.ToUpper().Contains("ID_PAUSED"))
         {
            _isIdentifying = false;
            foreach (var x in _mediaIdDict)
            {
               System.Console.WriteLine(x);
            }
            VideoLengthChanged(Convert.ToDouble(_mediaIdDict["length"]));
         }
         else
         {
            foreach (Match match in _idTagRegex.Matches(output.ToUpper()))
            {
               GroupCollection groups = match.Groups;
               System.Console.ForegroundColor = System.ConsoleColor.Green;
               System.Console.WriteLine(string.Format("{0}::{1}", groups[1].Value, groups[2].Value));
               _mediaIdDict[groups[1].Value.ToLower()] = groups[2].Value;
               System.Console.ResetColor();
            }
         }
      }

      private void ReadStdErr(object sender, DataReceivedEventArgs e)
      {
         OnErrorReceived(e.Data);
      }

   }
}
